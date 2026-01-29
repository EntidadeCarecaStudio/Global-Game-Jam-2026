using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("State")]
    public bool isCleared = false;
    public bool isActive = false; // O jogador está dentro?
    public Room.RoomType roomType;

    [Header("References")]
    public List<DoorController> myDoors = new List<DoorController>();
    public List<GameObject> myEnemies = new List<GameObject>();

    void Start()
    {
        // Regra: Salas de Start e Corredores são sempre "seguras"
        if (roomType == Room.RoomType.Start || roomType == Room.RoomType.Corridor)
        {
            isCleared = true;
        }

        // Estado Inicial: Portas ABERTAS esperando o jogador (exceto se for Start, que já começa aberta mesmo)
        OpenAllDoors();
    }

    void Update()
    {
        // Se a sala já foi limpa, não faz nada
        if (isCleared) return;

        // Só processa a lógica de combate se a sala estiver ATIVA (Jogador dentro)
        if (isActive)
        {
            // Remove inimigos mortos da lista
            myEnemies.RemoveAll(enemy => enemy == null);

            if (myEnemies.Count == 0)
            {
                RoomCleared();
            }
        }
    }

    // Detecta a entrada do Player
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // --- OTIMIZAÇÃO: Atualiza Visibilidade ---
            Room myRoom = GetComponent<Room>();
            if (myRoom != null && DungeonVisibilityManager.Instance != null)
            {
                DungeonVisibilityManager.Instance.UpdateVisibility(myRoom);
            }
            // -----------------------------------------

            if (isCleared || isActive) return;

            Debug.Log("Player entrou na sala: " + gameObject.name);
            StartCombat();
        }
    }

    void StartCombat()
    {
        isActive = true;
        CloseAllDoors(); 

        Debug.Log($"Iniciando combate na sala {gameObject.name} com {myEnemies.Count} inimigos.");

        // --- NOVO: Acorda os inimigos ---
        foreach (GameObject enemyObj in myEnemies)
        {
            if (enemyObj != null)
            {
                // Tenta pegar o controlador
                EnemySpawnController spawner = enemyObj.GetComponent<EnemySpawnController>();
                
                if (spawner != null)
                {
                    // Faz a animação bonita
                    spawner.StartSpawnSequence();
                }
                else
                {
                    // Fallback: Se o inimigo não tiver o script, garante que ative (se estiver desativado)
                    enemyObj.SetActive(true); 
                }
            }
        }
    }

    void RoomCleared()
    {
        isCleared = true;
        isActive = false;
        OpenAllDoors(); // LIBERA O JOGADOR!
        Debug.Log("Sala limpa!");
    }

    void OpenAllDoors()
    {
        foreach (var door in myDoors)
        {
            if (door != null) door.OpenDoor();
        }
    }

    void CloseAllDoors()
    {
        foreach (var door in myDoors)
        {
            if (door != null) door.CloseDoor();
        }
    }
}