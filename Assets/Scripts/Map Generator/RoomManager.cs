using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("State")]
    public bool isCleared = false;
    public bool isActive = false;
    public Room.RoomType roomType;

    [Header("References")]
    public List<DoorController> myDoors = new List<DoorController>();
    public List<GameObject> myEnemies = new List<GameObject>();

    void Start()
    {
        // Start e Corredores são seguros
        if (roomType == Room.RoomType.Start || roomType == Room.RoomType.Corridor)
        {
            isCleared = true;
        }

        // --- MUDANÇA IMPORTANTE ---
        // Não chamamos mais OpenAllDoors() aqui.
        // Deixamos as portas destravadas (padrão) mas FECHADAS visualmente.
        // O DoorController abrirá sozinho quando o player chegar perto.
        
        // Apenas garantimos que elas saibam que não estão travadas
        UnlockAllDoors();
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
            // Atualiza o Fog of War
            Room myRoom = GetComponent<Room>();
            if (myRoom != null && DungeonVisibilityManager.Instance != null)
            {
                DungeonVisibilityManager.Instance.UpdateVisibility(myRoom);
            }

            if (isCleared || isActive) return;

            StartCombat();
        }
    }

    void StartCombat()
    {
        isActive = true;
        LockAllDoors(); // TRANCAR AS PORTAS!
        
        // Acorda os inimigos
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
        UnlockAllDoors(); // LIBERAR AS PORTAS!
    }

    // --- Novos métodos de controle ---

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