using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Player & Main Boss")]
    public GameObject playerPrefab;      // Arraste o Player aqui
    public GameObject mainBossEnemyPrefab; // Arraste o Boss Final aqui

    [Header("Prefabs - Standard")]
    public Room startRoomPrefab;
    public Room bossRoomPrefab;
    public Room[] roomPrefabs;
    public Room[] corridorPrefabs;
    public GameObject wallPrefab;
    public GameObject doorPrefab;

    [Header("Prefabs - MiniBoss Rooms (1 of each)")]
    public Room fireRoomPrefab;
    public Room iceRoomPrefab;
    public Room earthRoomPrefab;

    [Header("Prefabs - MiniBoss Enemies")]
    public GameObject fireBossPrefab;
    public GameObject iceBossPrefab;
    public GameObject earthBossPrefab;

    [Header("Generation Rules")]
    public int desiredRooms = 20;
    public int maxSimulationAttempts = 100;
    
    [Header("Collision Settings")]
    public float roomRadius = 8f; 
    public float gridSize = 10f; 

    [Header("Enemy Spawning")]
    public GameObject[] standardEnemyPrefabs; // Minions normais
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 3;
    [Range(0f, 1f)] public float enemySpawnChance = 0.8f;

    // --- VIRTUAL DATA STRUCTURES ---
    private class VirtualRoom
    {
        public Room prefabReference;
        public Vector3 position;
        public Quaternion rotation;
        public Room.RoomType type;
        public Room.RoomElement element; // Guardamos o elemento aqui
        
        public List<Vector3> connectorsPoints;
        public List<Quaternion> connectorsRots;
        public bool[] isConnectorUsed; 

        // --- NOVO: Rastreamento do Pai para poder "Podar" depois ---
        public int parentIndex = -1; 
        public int parentConnectorIndex = -1;
    }

    private Dictionary<Room, RoomDataCache> prefabCache = new Dictionary<Room, RoomDataCache>();
    private struct RoomDataCache 
    { 
        public List<TransformData> connectors; 
        public int connectorCount;
    }
    private struct TransformData { public Vector3 localPos; public Quaternion localRot; }

    void Start()
    {
        // Cache de todos os prefabs, incluindo os novos minibosses
        CachePrefabData(startRoomPrefab);
        CachePrefabData(bossRoomPrefab);
        CachePrefabData(fireRoomPrefab);
        CachePrefabData(iceRoomPrefab);
        CachePrefabData(earthRoomPrefab);

        foreach (var r in roomPrefabs) CachePrefabData(r);
        foreach (var c in corridorPrefabs) CachePrefabData(c);

        GenerateDungeon();
    }

    void CachePrefabData(Room prefab)
    {
        if(prefab == null || prefabCache.ContainsKey(prefab)) return;

        Room temp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        temp.gameObject.SetActive(false);
        
        List<TransformData> connectorData = new List<TransformData>();
        foreach(var conn in temp.connectors)
        {
            connectorData.Add(new TransformData { localPos = conn.localPosition, localRot = conn.localRotation });
        }
        
        prefabCache.Add(prefab, new RoomDataCache { 
            connectors = connectorData, 
            connectorCount = connectorData.Count 
        });
        
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
            
            // --- NOVO: Remove corredores inúteis antes de construir ---
            CleanupDanglingCorridors(finalLayout);
            // ---------------------------------------------------------
            
            Debug.Log($"Dungeon generated successfully on attempt {attempts}");
            BuildDungeon(finalLayout);
        }
        else
        {
            Debug.LogError("Failed to generate dungeon. Try reducing Room Radius or increasing max attempts.");
        }
    }

    List<VirtualRoom> SimulateDungeon()
    {
        List<VirtualRoom> virtualRooms = new List<VirtualRoom>();
        
        // 1. Setup Start Room
        VirtualRoom vStart = CreateVirtualRoom(startRoomPrefab, Vector3.zero, Quaternion.identity);
        virtualRooms.Add(vStart);

        List<(int roomIdx, int connIdx)> pendingConnectors = new List<(int, int)>();
        for(int i=0; i < vStart.connectorsPoints.Count; i++) pendingConnectors.Add((0, i));

        // 2. Lista de Minibosses Obrigatórios
        List<Room> requiredMiniBosses = new List<Room> { fireRoomPrefab, iceRoomPrefab, earthRoomPrefab };
        // Embaralha para a ordem não ser sempre Fogo -> Gelo -> Terra
        requiredMiniBosses = requiredMiniBosses.OrderBy(x => Random.value).ToList();

        int roomsCreated = 1;
        int safety = 0;

        while (roomsCreated < desiredRooms && pendingConnectors.Count > 0 && safety < 1000)
        {
            safety++;

            int pendingIndex = Random.Range(0, pendingConnectors.Count);
            var (parentIdx, parentConnIdx) = pendingConnectors[pendingIndex];
            VirtualRoom parentRoom = virtualRooms[parentIdx];

            // --- LÓGICA DE SELEÇÃO DE CANDIDATO ---
            Room prefabToTry = null;
            bool tryingSpecial = false;

            // Regra: Se temos Minibosses pendentes, tente spawnar um deles com 40% de chance
            // OU se estamos ficando sem espaço (roomsCreated > 70% do total), force o spawn deles.
            bool forceSpecial = roomsCreated > (desiredRooms * 0.7f);
            
            if (requiredMiniBosses.Count > 0 && parentRoom.type == Room.RoomType.Corridor && (Random.value < 0.4f || forceSpecial))
            {
                prefabToTry = requiredMiniBosses[0];
                tryingSpecial = true;
            }
            else
            {
                // Seleção Normal (Sala ou Corredor)
                Room[] candidates = (parentRoom.type == Room.RoomType.Corridor) ? roomPrefabs : corridorPrefabs;
                if (parentRoom.type == Room.RoomType.Start || parentRoom.type == Room.RoomType.Room || parentRoom.type == Room.RoomType.MiniBoss) 
                    candidates = corridorPrefabs;
                
                prefabToTry = candidates[Random.Range(0, candidates.Length)];
            }

            RoomDataCache entryData = prefabCache[prefabToTry];
            
            // Lógica de Transformada (igual ao anterior)
            TransformData childEntry = entryData.connectors[0]; 
            Vector3 targetPos = parentRoom.connectorsPoints[parentConnIdx];
            Quaternion targetRot = parentRoom.connectorsRots[parentConnIdx];

            Quaternion requiredRot = (targetRot * Quaternion.Euler(0, 180, 0)) * Quaternion.Inverse(childEntry.localRot);
            Vector3 requiredPos = targetPos - (requiredRot * childEntry.localPos);

            if (IsPositionValid(requiredPos, virtualRooms))
            {
                VirtualRoom vNew = CreateVirtualRoom(prefabToTry, requiredPos, requiredRot);

                // --- NOVO: Registra quem criou esta sala ---
                vNew.parentIndex = parentIdx;
                vNew.parentConnectorIndex = parentConnIdx;
                // ------------------------------------------

                virtualRooms.Add(vNew);
                roomsCreated++;
                int newRoomIdx = virtualRooms.Count - 1;

                parentRoom.isConnectorUsed[parentConnIdx] = true;
                vNew.isConnectorUsed[0] = true;

                for (int i = 1; i < vNew.connectorsPoints.Count; i++)
                    pendingConnectors.Add((newRoomIdx, i));

                pendingConnectors.RemoveAt(pendingIndex);

                // Se conseguimos colocar o Miniboss, removemos da lista de obrigatórios
                if (tryingSpecial)
                {
                    requiredMiniBosses.RemoveAt(0);
                }
            }
        }

        // Se o loop terminou e ainda faltam minibosses, essa simulação falhou (se você considerar estrito)
        // Para este exemplo, vamos considerar sucesso se a Boss Room couber, mas o ideal é verificar requiredMiniBosses.Count == 0

        // 3. Boss Placement (Mesma lógica)
        if (roomsCreated >= desiredRooms)
        {
            float maxDist = 0;
            int bestConnIndex = -1;

            for(int i=0; i < pendingConnectors.Count; i++)
            {
                var (rIdx, cIdx) = pendingConnectors[i];
                if (virtualRooms[rIdx].type == Room.RoomType.Corridor)
                {
                    float dist = Vector3.Distance(Vector3.zero, virtualRooms[rIdx].connectorsPoints[cIdx]);
                    if(dist > maxDist) { maxDist = dist; bestConnIndex = i; }
                }
            }

            if (bestConnIndex != -1)
            {
                var (pIdx, cIdx) = pendingConnectors[bestConnIndex];
                VirtualRoom pRoom = virtualRooms[pIdx];
                RoomDataCache bossData = prefabCache[bossRoomPrefab];
                TransformData entryData = bossData.connectors[0];

                Quaternion bossRot = (pRoom.connectorsRots[cIdx] * Quaternion.Euler(0, 180, 0)) * Quaternion.Inverse(entryData.localRot);
                Vector3 bossPos = pRoom.connectorsPoints[cIdx] - (bossRot * entryData.localPos);

                if (IsPositionValid(bossPos, virtualRooms))
                {
                    VirtualRoom vBoss = CreateVirtualRoom(bossRoomPrefab, bossPos, bossRot);
                    pRoom.isConnectorUsed[cIdx] = true;
                    vBoss.isConnectorUsed[0] = true;
                    virtualRooms.Add(vBoss);
                    return virtualRooms;
                }
            }
        }
        return null;
    }

    bool IsPositionValid(Vector3 pos, List<VirtualRoom> existingRooms)
    {
        foreach(var room in existingRooms)
        {
            if(Vector3.Distance(pos, room.position) < roomRadius) return false;
        }
        return true;
    }

    VirtualRoom CreateVirtualRoom(Room prefab, Vector3 pos, Quaternion rot)
    {
        VirtualRoom vr = new VirtualRoom();
        vr.prefabReference = prefab;
        vr.position = pos;
        vr.rotation = rot;
        vr.type = prefab.type;
        vr.element = prefab.element; // Copia o elemento do prefab
        vr.connectorsPoints = new List<Vector3>();
        vr.connectorsRots = new List<Quaternion>();

        var cachedData = prefabCache[prefab];
        vr.isConnectorUsed = new bool[cachedData.connectorCount];

        foreach(var c in cachedData.connectors)
        {
            vr.connectorsPoints.Add(pos + (rot * c.localPos));
            vr.connectorsRots.Add(rot * c.localRot);
        }

        return vr;
    }

    void BuildDungeon(List<VirtualRoom> layout)
    {
        List<Room> instantiatedRooms = new List<Room>();

        // 1. Instanciação Física
        foreach (var vr in layout)
        {
            Room newRoom = Instantiate(vr.prefabReference, vr.position, vr.rotation, transform);
            
            // Configuração do Manager (Inimigos/Portas)
            RoomManager roomManager = newRoom.gameObject.GetComponent<RoomManager>();
            if (roomManager == null) roomManager = newRoom.gameObject.AddComponent<RoomManager>();
            roomManager.roomType = vr.type; 
            
            // Paredes e Portas
            for(int i=0; i < vr.isConnectorUsed.Length; i++)
            {
                Transform connectorTrans = newRoom.connectors[i];
                if (!vr.isConnectorUsed[i])
                {
                    if (wallPrefab != null)
                    {
                        Instantiate(wallPrefab, connectorTrans.position + (Vector3.up * 2f), connectorTrans.rotation, transform);
                    }
                }
                else
                {
                    if (vr.type != Room.RoomType.Corridor && doorPrefab != null)
                    {
                        GameObject doorObj = Instantiate(doorPrefab, connectorTrans.position, connectorTrans.rotation, transform);
                        DoorController doorCtrl = doorObj.GetComponent<DoorController>();
                        if (doorCtrl != null) roomManager.myDoors.Add(doorCtrl);
                    }
                }
            }

            // Spawn System
            if (vr.type == Room.RoomType.Start) SpawnSingleEntity(newRoom, playerPrefab, null);
            else if (vr.type == Room.RoomType.Boss) SpawnSingleEntity(newRoom, mainBossEnemyPrefab, roomManager);
            else if (vr.type == Room.RoomType.MiniBoss) SpawnMiniBossEnemies(newRoom, vr.element, roomManager);
            else if (vr.type == Room.RoomType.Room && Random.value <= enemySpawnChance) SpawnStandardEnemies(newRoom, roomManager);

            instantiatedRooms.Add(newRoom);
        }

        // 2. Linkagem de Vizinhos (ESSENCIAL PARA A VISIBILIDADE)
        for (int i = 0; i < layout.Count; i++)
        {
            VirtualRoom vr = layout[i];
            
            // Conecta Filho <-> Pai
            if (vr.parentIndex >= 0 && vr.parentIndex < instantiatedRooms.Count)
            {
                Room currentPhysRoom = instantiatedRooms[i];
                Room parentPhysRoom = instantiatedRooms[vr.parentIndex];

                if (!currentPhysRoom.neighbors.Contains(parentPhysRoom)) currentPhysRoom.neighbors.Add(parentPhysRoom);
                if (!parentPhysRoom.neighbors.Contains(currentPhysRoom)) parentPhysRoom.neighbors.Add(currentPhysRoom);
            }
        }

        // 3. Inicializa o Sistema de Visibilidade Otimizado
        if (DungeonVisibilityManager.Instance == null)
        {
            GameObject managerObj = new GameObject("DungeonVisibilityManager");
            managerObj.AddComponent<DungeonVisibilityManager>();
        }

        DungeonVisibilityManager.Instance.Initialize(instantiatedRooms);
        
        // Força a sala inicial a aparecer (junto com seus vizinhos imediatos)
        if (instantiatedRooms.Count > 0)
        {
            DungeonVisibilityManager.Instance.UpdateVisibility(instantiatedRooms[0]);
        }
    }

    // --- MÉTODOS DE SPAWN ---

    void SpawnMiniBossEnemies(Room roomInstance, Room.RoomElement element, RoomManager manager)
    {
        if (roomInstance.spawnPoints == null || roomInstance.spawnPoints.Count == 0) return;

        List<Transform> availablePoints = new List<Transform>(roomInstance.spawnPoints);
        
        GameObject bossPrefabToSpawn = null;
        switch (element)
        {
            case Room.RoomElement.Fire: bossPrefabToSpawn = fireBossPrefab; break;
            case Room.RoomElement.Ice: bossPrefabToSpawn = iceBossPrefab; break;
            case Room.RoomElement.Earth: bossPrefabToSpawn = earthBossPrefab; break;
        }

        if (bossPrefabToSpawn != null)
        {
            Transform bossPoint = availablePoints[0];
            GameObject bossObj = Instantiate(bossPrefabToSpawn, bossPoint.position, bossPoint.rotation, roomInstance.transform);
            
            // --- NOVO: Registra o Boss ---
            if (manager != null) SetupEnemySpawn(bossObj, manager);

            availablePoints.RemoveAt(0);
        }

        int minionsToSpawn = Random.Range(1, availablePoints.Count + 1);
        
        for(int i = 0; i < minionsToSpawn; i++)
        {
            if (availablePoints.Count == 0) break;
            if (standardEnemyPrefabs == null || standardEnemyPrefabs.Length == 0) break;

            int idx = Random.Range(0, availablePoints.Count);
            GameObject minionPrefab = standardEnemyPrefabs[Random.Range(0, standardEnemyPrefabs.Length)];
            
            GameObject minionObj = Instantiate(minionPrefab, availablePoints[idx].position, availablePoints[idx].rotation, roomInstance.transform);
            
            // --- NOVO: Registra o Minion ---
            if (manager != null) SetupEnemySpawn(minionObj, manager);
            
            availablePoints.RemoveAt(idx);
        }
    }

    // Adicionado parâmetro RoomManager manager
    void SpawnStandardEnemies(Room roomInstance, RoomManager manager)
    {
        if (standardEnemyPrefabs == null || standardEnemyPrefabs.Length == 0) return;
        if (roomInstance.spawnPoints == null || roomInstance.spawnPoints.Count == 0) return;

        int maxCount = Mathf.Min(maxEnemiesPerRoom, roomInstance.spawnPoints.Count);
        int enemyCount = Random.Range(minEnemiesPerRoom, maxCount + 1);
        List<Transform> availablePoints = new List<Transform>(roomInstance.spawnPoints);

        for (int i = 0; i < enemyCount; i++)
        {
            if (availablePoints.Count == 0) break;
            int idx = Random.Range(0, availablePoints.Count);
            GameObject enemyPrefab = standardEnemyPrefabs[Random.Range(0, standardEnemyPrefabs.Length)];
            
            // Instancia
            GameObject newEnemy = Instantiate(enemyPrefab, availablePoints[idx].position, availablePoints[idx].rotation, roomInstance.transform);
            
            // --- ALTERADO: Usa o método de setup ---
            SetupEnemySpawn(newEnemy, manager);
            // ---------------------------------------

            availablePoints.RemoveAt(idx);
        }
    }

    void CleanupDanglingCorridors(List<VirtualRoom> layout)
    {
        // Percorre de trás para frente para poder remover itens da lista com segurança
        // Começamos do final, ignorando a sala 0 (Start)
        for (int i = layout.Count - 1; i > 0; i--)
        {
            VirtualRoom currentRoom = layout[i];

            // A lógica se aplica se for um CORREDOR
            if (currentRoom.type == Room.RoomType.Corridor)
            {
                // Verifica se o corredor tem alguma saída usada (além da entrada [0])
                bool hasChild = false;
                for (int c = 1; c < currentRoom.isConnectorUsed.Length; c++)
                {
                    if (currentRoom.isConnectorUsed[c])
                    {
                        hasChild = true;
                        break;
                    }
                }

                // Se não tem filhos, é um corredor sem saída (beco) ou sobreposto
                if (!hasChild)
                {
                    // 1. Identifica o Pai
                    if (currentRoom.parentIndex >= 0 && currentRoom.parentIndex < layout.Count)
                    {
                        VirtualRoom parentRoom = layout[currentRoom.parentIndex];
                        int connIdx = currentRoom.parentConnectorIndex;

                        // 2. Avisa o pai que a conexão não está mais em uso
                        // Isso fará o script BuildDungeon instanciar uma Parede neste local
                        parentRoom.isConnectorUsed[connIdx] = false;
                    }

                    // 3. Remove este corredor da lista final
                    layout.RemoveAt(i);
                }
            }
        }
    }


    // Adicionado parâmetro RoomManager manager (pode ser nulo para o Player)
    void SpawnSingleEntity(Room roomInstance, GameObject entityPrefab, RoomManager manager)
    {
        if (entityPrefab == null) return;
        
        if (roomInstance.spawnPoints == null || roomInstance.spawnPoints.Count == 0) 
        {
            Debug.LogWarning($"A sala {roomInstance.name} não tem Spawn Point!");
            return;
        }

        Transform spawnPoint = roomInstance.spawnPoints[0];
        GameObject entityObj = Instantiate(entityPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // --- NOVO: Se tiver manager (caso do Boss), registra ---
        if (manager != null) SetupEnemySpawn(entityObj, manager);
    }


    // Método auxiliar para preparar o inimigo logo após instanciar
    void SetupEnemySpawn(GameObject enemyObj, RoomManager manager)
    {
        // Adiciona à lista do manager
        if (manager != null) manager.myEnemies.Add(enemyObj);

        // Verifica se tem o componente de Spawn
        EnemySpawnController spawnCtrl = enemyObj.GetComponent<EnemySpawnController>();
        
        // Se não tiver (esquecimento do dev), adiciona automaticamente
        if (spawnCtrl == null) spawnCtrl = enemyObj.AddComponent<EnemySpawnController>();

        // Prepara (enterra o inimigo e desativa AI)
        spawnCtrl.PrepareForSpawn();
    }
}