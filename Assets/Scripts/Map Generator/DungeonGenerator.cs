using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public Room startRoomPrefab;
    public Room bossRoomPrefab;
    public Room[] roomPrefabs;      
    public Room[] corridorPrefabs;  
    public GameObject wallPrefab;

    [Header("Regras de Geração")]
    public int desiredRooms = 20;   
    public int maxSimulationAttempts = 100;
    
    // Tolerância de sobreposição (0.1 = 10%)
    [Range(0f, 1f)] public float maxOverlapPercentage = 0.1f; 

    // --- ESTRUTURAS DE DADOS VIRTUAIS ---
    
    private class VirtualRoom
    {
        public Room prefabReference;
        public Vector3 position;
        public Quaternion rotation;
        public Room.RoomType type;
        public List<Vector3> availableConnectorsPoints; 
        public List<Quaternion> availableConnectorsRots; 
        public Bounds worldBounds; // <--- NOVO: Armazena o volume físico no mundo
    }

    private struct PrefabData 
    { 
        public List<TransformData> connectors;
        public Bounds localBounds; // <--- NOVO: Armazena o volume local do prefab
    }

    private struct TransformData { public Vector3 localPos; public Quaternion localRot; }

    // Cache atualizado
    private Dictionary<Room, PrefabData> prefabCache = new Dictionary<Room, PrefabData>();

    void Start()
    {
        // 1. Pré-processamento: Lê geometria e BOUNDS
        CachePrefabData(startRoomPrefab);
        CachePrefabData(bossRoomPrefab);
        foreach (var r in roomPrefabs) CachePrefabData(r);
        foreach (var c in corridorPrefabs) CachePrefabData(c);

        GenerateDungeon();
    }

    void CachePrefabData(Room prefab)
    {
        Room temp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        temp.gameObject.SetActive(false); // Garante que não interfira na física durante o setup
        
        // Coleta conectores
        List<TransformData> connData = new List<TransformData>();
        foreach(var conn in temp.connectors)
        {
            connData.Add(new TransformData { localPos = conn.localPosition, localRot = conn.localRotation });
        }

        // --- NOVO: Coleta Bounds (Caixa de Colisão) ---
        Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        // Tenta pegar bounds dos Colliders primeiro, se não tiver, pega dos Renderers
        Collider[] colliders = temp.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            combinedBounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) combinedBounds.Encapsulate(colliders[i].bounds);
        }
        else
        {
            Renderer[] rends = temp.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                combinedBounds = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) combinedBounds.Encapsulate(rends[i].bounds);
            }
        }
        
        // Ajusta o bounds para ser relativo ao pivô do objeto (local space)
        // O bounds.center retornado pelo Unity é em World Space, mas como instanciamos em (0,0,0), funciona como local,
        // exceto se o pivô do modelo 3D estiver deslocado. Vamos assumir pivô correto.
        
        PrefabData data = new PrefabData 
        { 
            connectors = connData,
            localBounds = combinedBounds
        };

        prefabCache.Add(prefab, data);
        DestroyImmediate(temp.gameObject);
    }

    void GenerateDungeon()
    {
        List<VirtualRoom> finalLayout = null;
        int attempts = 0;

        while (finalLayout == null && attempts < maxSimulationAttempts)
        {
            attempts++;
            finalLayout = SimulateDungeon();
        }

        if (finalLayout != null)
        {
            Debug.Log($"Dungeon gerada na tentativa {attempts}.");
            BuildDungeon(finalLayout);
        }
        else
        {
            Debug.LogError("Falha Crítica: Não foi possível gerar dungeon sem sobreposições excessivas.");
        }
    }

    List<VirtualRoom> SimulateDungeon()
    {
        List<VirtualRoom> virtualRooms = new List<VirtualRoom>();
        // Lista de conectores pendentes: (IndiceSala, IndiceConector)
        List<(int rIdx, int cIdx)> pendingConnectors = new List<(int, int)>();

        // 1. Sala Inicial
        VirtualRoom vStart = CreateVirtualRoom(startRoomPrefab, Vector3.zero, Quaternion.identity);
        virtualRooms.Add(vStart);
        
        for(int i=0; i < vStart.availableConnectorsPoints.Count; i++) pendingConnectors.Add((0, i));

        int roomsCreated = 1; 
        int safety = 0;

        // Loop de Geração
        while (roomsCreated < desiredRooms && pendingConnectors.Count > 0 && safety < 1000)
        {
            safety++;
            
            // Pega um conector aleatório
            int rndIndex = Random.Range(0, pendingConnectors.Count);
            var (parentIdx, parentConnIdx) = pendingConnectors[rndIndex];
            VirtualRoom parentRoom = virtualRooms[parentIdx];

            // TENTA GERAR A CADEIA: [CORREDOR] -> [SALA]
            // Previsão de dois passos para evitar dead-ends curtos
            
            // A. Escolhe um Corredor Virtual
            Room corridorPrefab = corridorPrefabs[Random.Range(0, corridorPrefabs.Length)];
            VirtualRoom vCorridor = TryCalculateFit(parentRoom, parentConnIdx, corridorPrefab);

            if (vCorridor != null && !CheckOverlap(vCorridor, virtualRooms))
            {
                // B. Se o corredor cabe, tentamos colocar uma Sala na ponta dele
                // Precisamos achar o conector de saída do corredor (geralmente o oposto ao de entrada)
                // Assumindo: Entrada é index 0, Saída é index 1 (para corredores retos simples)
                // Para ser genérico, pegamos qualquer conector do corredor que não seja o de entrada
                
                // Mas espera: O vCorridor ainda não está na lista, seus conectores mundiais não foram calculados além da entrada.
                // Vamos calcular temporariamente.
                
                // Escolhe uma sala aleatória
                Room roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
                
                // Conector de saída do corredor: vamos tentar todos exceto o 0 (que conectou no pai)
                bool chainSuccess = false;
                VirtualRoom vRoom = null;

                for (int c = 1; c < vCorridor.availableConnectorsPoints.Count; c++)
                {
                    vRoom = TryCalculateFit(vCorridor, c, roomPrefab);
                    
                    // Checagem CRÍTICA: 
                    // 1. A Sala cabe? 
                    // 2. A Sala não bate no próprio corredor que estamos criando? (Raro, mas possível em 'U')
                    if (vRoom != null && !CheckOverlap(vRoom, virtualRooms) && !CheckOverlap(vRoom, new List<VirtualRoom>{vCorridor}))
                    {
                        chainSuccess = true;
                        break; // Achamos um par válido!
                    }
                }

                if (chainSuccess)
                {
                    // SUCESSO! Adiciona ambos (Corredor e Sala)
                    
                    // 1. Adiciona Corredor
                    virtualRooms.Add(vCorridor);
                    int corridorIdx = virtualRooms.Count - 1;
                    
                    // Adiciona conectores do corredor (caso tenha ramificações extras), exceto o usado pela sala
                    // (Simplificação: adicionamos todos e removemos o usado depois, ou deixamos a lógica de build fechar)
                    
                    // 2. Adiciona Sala
                    virtualRooms.Add(vRoom);
                    int roomIdx = virtualRooms.Count - 1;
                    roomsCreated++;

                    // Adiciona conectores da NOVA SALA à lista de pendentes
                    for (int k = 1; k < vRoom.availableConnectorsPoints.Count; k++)
                    {
                        pendingConnectors.Add((roomIdx, k));
                    }

                    // Remove o conector do pai usado
                    pendingConnectors.RemoveAt(rndIndex);
                }
                else
                {
                    // Falha na previsão da sala. O corredor cabe, mas a sala na ponta não.
                    // Estratégia: Desistimos desse conector POR AGORA? Ou tentamos outro prefab?
                    // Para evitar loop infinito, apenas passamos a vez.
                }
            }
        }

        // Tenta gerar Boss (Obrigatório) no final
        if (roomsCreated >= desiredRooms)
        {
            if (TryPlaceBoss(virtualRooms, pendingConnectors)) 
                return virtualRooms;
        }

        return null; // Falha se não atingiu meta ou boss
    }

    // Tenta calcular posição/rotação. Retorna null se erro matemático, retorna VirtualRoom (sem checar colisão ainda) se der certo.
    VirtualRoom TryCalculateFit(VirtualRoom parent, int parentConnIdx, Room prefabToTry)
    {
        Vector3 targetPos = parent.availableConnectorsPoints[parentConnIdx];
        Quaternion targetRot = parent.availableConnectorsRots[parentConnIdx];

        TransformData entryConnData = prefabCache[prefabToTry].connectors[0];

        // Rotação para alinhar (Opposite direction)
        Quaternion requiredRotation = (targetRot * Quaternion.Euler(0, 180, 0)) * Quaternion.Inverse(entryConnData.localRot);
        Vector3 requiredPosition = targetPos - (requiredRotation * entryConnData.localPos);

        return CreateVirtualRoom(prefabToTry, requiredPosition, requiredRotation);
    }

    VirtualRoom CreateVirtualRoom(Room prefab, Vector3 pos, Quaternion rot)
    {
        VirtualRoom vr = new VirtualRoom();
        vr.prefabReference = prefab;
        vr.position = pos;
        vr.rotation = rot;
        vr.type = prefab.type;
        vr.availableConnectorsPoints = new List<Vector3>();
        vr.availableConnectorsRots = new List<Quaternion>();

        var data = prefabCache[prefab];

        // Calcula Conectores no Mundo
        foreach(var c in data.connectors)
        {
            vr.availableConnectorsPoints.Add(pos + (rot * c.localPos));
            vr.availableConnectorsRots.Add(rot * c.localRot);
        }

        // Calcula Bounds no Mundo (AABB Aproximado)
        // Rotacionar um AABB é complexo, a Unity faz isso recalculando o bounding box que engloba a rotação
        Vector3 center = pos + (rot * data.localBounds.center);
        
        // Simples aproximação de tamanho rotacionado para 90 graus
        // (Se suas salas giram em ângulos arbitrários, precisaria de lógica OBB, mas para dungeon geralmente é 90)
        Vector3 size = data.localBounds.size;
        
        // Verifica se a rotação troca os eixos X e Z (aproximadamente)
        float yRot = Quaternion.Angle(Quaternion.identity, rot); // Simplificado
        // Maneira mais robusta de ver se girou 90 graus no Y:
        Vector3 rotatedSize = Mathf.Abs(Vector3.Dot(transform.forward, rot * Vector3.forward)) < 0.5f 
            ? new Vector3(size.z, size.y, size.x) 
            : size;

        vr.worldBounds = new Bounds(center, rotatedSize);
        // Reduz levemente o bounds para permitir "encostar" paredes sem contar como sobreposição
        vr.worldBounds.Expand(-0.1f); 

        return vr;
    }

    // --- LÓGICA DE SOBREPOSIÇÃO (10%) ---
    bool CheckOverlap(VirtualRoom newRoom, List<VirtualRoom> existingRooms)
    {
        foreach(var existing in existingRooms)
        {
            if (newRoom.worldBounds.Intersects(existing.worldBounds))
            {
                // Calcula volume da interseção
                float intersectionVolume = GetIntersectionVolume(newRoom.worldBounds, existing.worldBounds);
                
                // Calcula volume da nova sala
                float newRoomVolume = newRoom.worldBounds.size.x * newRoom.worldBounds.size.y * newRoom.worldBounds.size.z;

                // Se a interseção for maior que X% do volume da sala nova, bloqueia
                if (newRoomVolume > 0 && (intersectionVolume / newRoomVolume) > maxOverlapPercentage)
                {
                    return true; // Sobreposição detectada
                }
            }
        }
        return false;
    }

    float GetIntersectionVolume(Bounds b1, Bounds b2)
    {
        Vector3 min = Vector3.Max(b1.min, b2.min);
        Vector3 max = Vector3.Min(b1.max, b2.max);

        float x = Mathf.Max(0, max.x - min.x);
        float y = Mathf.Max(0, max.y - min.y);
        float z = Mathf.Max(0, max.z - min.z);

        return x * y * z;
    }

    bool TryPlaceBoss(List<VirtualRoom> rooms, List<(int, int)> connectors)
    {
        // Tenta achar o ponto mais distante
        float maxDist = 0;
        int bestIdx = -1;
        
        for(int i=0; i < connectors.Count; i++)
        {
            var c = connectors[i];
            float d = Vector3.Distance(Vector3.zero, rooms[c.Item1].availableConnectorsPoints[c.Item2]);
            if(d > maxDist) { maxDist = d; bestIdx = i; }
        }

        if (bestIdx != -1)
        {
            var (pIdx, cIdx) = connectors[bestIdx];
            VirtualRoom boss = TryCalculateFit(rooms[pIdx], cIdx, bossRoomPrefab);
            if(boss != null && !CheckOverlap(boss, rooms))
            {
                rooms.Add(boss);
                return true;
            }
        }
        return false;
    }

    void BuildDungeon(List<VirtualRoom> layout)
    {
        foreach (var vr in layout)
        {
            Room newRoom = Instantiate(vr.prefabReference, vr.position, vr.rotation, transform);
            StartCoroutine(CloseHolesRoutine(newRoom));
        }
    }

    System.Collections.IEnumerator CloseHolesRoutine(Room room)
    {
        yield return new WaitForFixedUpdate(); 
        foreach(Transform connector in room.connectors)
        {
            // Raycast físico para ver se tem sala conectada
            // Como agora garantimos geometria via bounds, o raycast é seguro
            if(!Physics.Raycast(connector.position, connector.forward, 2f)) // Raycast curto para fora
            {
                 // Nota: Isso é um check simplista. O ideal é verificar se existe um outro conector alinhado.
                 // Mas para fechar buracos visuais, funciona.
                if(wallPrefab) Instantiate(wallPrefab, connector.position, connector.rotation, transform);
            }
        }
    }
}
