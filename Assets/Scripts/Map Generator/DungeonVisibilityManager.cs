using UnityEngine;
using System.Collections.Generic;

public class DungeonVisibilityManager : MonoBehaviour
{
    public static DungeonVisibilityManager Instance;
    
    private List<Room> allRooms = new List<Room>();
    public int viewDepth = 2; 

    void Awake()
    {
        Instance = this;
    }

    public void Initialize(List<Room> generatedRooms)
    {
        allRooms = generatedRooms;
        
        // Inicialização Suave:
        // Mantemos os GameObjects ativos (SetActive true), 
        // mas desligamos os renderizadores.
        foreach (var r in allRooms)
        {
            r.gameObject.SetActive(true); // Garante que scripts/physics rodem
            r.ToggleVisibility(false);    // Mas começa invisível
        }
    }

    public void UpdateVisibility(Room currentRoom)
    {
        HashSet<Room> visibleRooms = new HashSet<Room>();
        CollectNeighbors(currentRoom, viewDepth, visibleRooms);

        foreach (var room in allRooms)
        {
            bool shouldBeVisible = visibleRooms.Contains(room);
            
            // AQUI ESTÁ A MUDANÇA:
            // Usamos o método otimizado em vez de desligar o objeto inteiro
            room.ToggleVisibility(shouldBeVisible);
        }
    }

    void CollectNeighbors(Room room, int depth, HashSet<Room> collected)
    {
        if (room == null || depth < 0) return;
        collected.Add(room);

        if (depth > 0)
        {
            foreach (var neighbor in room.neighbors)
            {
                CollectNeighbors(neighbor, depth - 1, collected);
            }
        }
    }
}