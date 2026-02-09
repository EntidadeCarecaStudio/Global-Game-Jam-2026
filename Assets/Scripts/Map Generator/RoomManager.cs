using UnityEngine;
using System.Collections.Generic;
using System; // Necessário para Action

public class RoomManager : MonoBehaviour
{
    [Header("State")]
    public bool isCleared = false;
    public bool isActive = false;
    public Room.RoomType roomType;

    [Header("References")]
    public List<DoorController> myDoors = new List<DoorController>();
    public List<GameObject> myEnemies = new List<GameObject>();

    // --- LÓGICA DE PROGRESSÃO ESTÁTICA ---
    // Mantemos um registro estático de quais minibosses foram mortos nesta sessão de jogo
    private static HashSet<Room.RoomElement> clearedMiniBossElements = new HashSet<Room.RoomElement>();

    // Evento para avisar a sala do Boss que ela foi liberada
    public static event Action OnBossUnlockConditionMet;

    public static void ResetProgression()
    {
        clearedMiniBossElements.Clear();
    }
    // --------------------------------------

    void OnEnable()
    {
        // Se eu sou a sala do Boss, quero saber quando os minibosses morreram
        if (roomType == Room.RoomType.Boss)
        {
            OnBossUnlockConditionMet += UnlockBossRoom;
        }
    }

    void OnDisable()
    {
        if (roomType == Room.RoomType.Boss)
        {
            OnBossUnlockConditionMet -= UnlockBossRoom;
        }
    }

    void Start()
    {
        // Start e Corredores são seguros
        if (roomType == Room.RoomType.Start || roomType == Room.RoomType.Corridor)
        {
            isCleared = true;
        }

        // --- ALTERAÇÃO: Lógica de Portas Inicial ---
        if (roomType == Room.RoomType.Boss)
        {
            // Se for Boss, TRANCAMOS as portas imediatamente e não abrimos
            LockAllDoors();
        }
        else
        {
            // Salas normais, start e corredores começam destrancadas (abrem por proximidade)
            UnlockAllDoors();
        }
    }

    void Update()
    {
        if (isCleared) return;

        if (isActive)
        {
            myEnemies.RemoveAll(enemy => enemy == null);
            if (myEnemies.Count == 0) RoomCleared();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Room myRoom = GetComponent<Room>();
            /*if (myRoom != null && DungeonVisibilityManager.Instance != null)
            {
                DungeonVisibilityManager.Instance.UpdateVisibility(myRoom);
            }*/

            if (isCleared || isActive) return;

            // Se for Boss Room e estiver trancada (Minibosses vivos), o player não deveria conseguir entrar.
            // Mas caso ele entre (teleporte/bug), iniciamos combate.
            StartCombat();
        }
    }

    void StartCombat()
    {
        isActive = true;
        LockAllDoors(); // Tranca para o player não fugir durante o combate

        foreach (GameObject enemyObj in myEnemies)
        {
            if (enemyObj != null)
            {
                EnemySpawnController spawner = enemyObj.GetComponent<EnemySpawnController>();
                if (spawner != null) spawner.StartSpawnSequence();
                else enemyObj.SetActive(true);
            }
        }
    }

    void RoomCleared()
    {
        isCleared = true;
        isActive = false;

        // --- ALTERAÇÃO: Verifica Minibosses ---
        if (roomType == Room.RoomType.MiniBoss)
        {
            Room myRoom = GetComponent<Room>();
            if (myRoom != null)
            {
                RegisterMiniBossDefeat(myRoom.element);
            }
        }
        // --------------------------------------

        UnlockAllDoors(); // Libera as portas após vencer o combate local
    }

    // --- LÓGICA DE CONTROLE DE FLUXO ---

    void RegisterMiniBossDefeat(Room.RoomElement element)
    {
        if (!clearedMiniBossElements.Contains(element))
        {
            clearedMiniBossElements.Add(element);
            Debug.Log($"MiniBoss de {element} derrotado! Progresso: {clearedMiniBossElements.Count}/3");

            // Verifica se temos os 3 elementos necessários
            if (clearedMiniBossElements.Contains(Room.RoomElement.Fire) &&
                clearedMiniBossElements.Contains(Room.RoomElement.Ice) &&
                clearedMiniBossElements.Contains(Room.RoomElement.Earth))
            {
                Debug.Log("TODOS OS MINIBOSSES DERROTADOS! ABRINDO SALA DO BOSS!");
                OnBossUnlockConditionMet?.Invoke();
            }
        }
    }

    // Método específico chamado apenas na Sala do Boss via Evento
    void UnlockBossRoom()
    {
        if (roomType == Room.RoomType.Boss)
        {
            // Destranca as portas para permitir a entrada do Player
            // Nota: Elas ainda respeitam a proximidade (DoorController), mas agora 'isLocked' é false
            UnlockAllDoors();
        }
    }

    // --- Helpers de Porta ---

    void UnlockAllDoors()
    {
        foreach (var door in myDoors)
        {
            if (door != null) door.UnlockDoor();
        }
    }

    void LockAllDoors()
    {
        foreach (var door in myDoors)
        {
            if (door != null) door.LockDoor();
        }
    }
}