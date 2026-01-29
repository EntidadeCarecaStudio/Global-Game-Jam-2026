using UnityEngine;
using System.Collections.Generic;

public class DungeonVisibilityManager : MonoBehaviour
{
    public static DungeonVisibilityManager Instance;
    
    // Lista mestre de todas as salas da dungeon
    private List<Room> allRooms = new List<Room>();
    
    // Define a profundidade: 2 significa (Sala Atual + Vizinhos + Vizinhos dos Vizinhos)
    public int viewDepth = 2; 

    void Awake()
    {
        Instance = this;
    }

    public void Initialize(List<Room> generatedRooms)
    {
        allRooms = generatedRooms;
        
        // Começa desativando tudo para garantir um estado limpo
        foreach (var r in allRooms) r.gameObject.SetActive(false);
    }

    public void UpdateVisibility(Room currentRoom)
    {
        // 1. Encontrar quais salas devem estar visíveis
        // Usamos HashSet para evitar duplicatas e busca rápida
        HashSet<Room> visibleRooms = new HashSet<Room>();
        
        // Função recursiva para coletar vizinhos até a profundidade X
        CollectNeighbors(currentRoom, viewDepth, visibleRooms);

        // 2. Aplicar o estado (Ativar o que deve, Desativar o resto)
        foreach (var room in allRooms)
        {
            bool shouldBeVisible = visibleRooms.Contains(room);
            
            // Só altera o SetActive se o estado for diferente para economizar chamadas
            if (room.gameObject.activeSelf != shouldBeVisible)
            {
                room.gameObject.SetActive(shouldBeVisible);
            }
        }
    }

    // Algoritmo de busca em profundidade (Recursive)
    void CollectNeighbors(Room room, int depth, HashSet<Room> collected)
    {
        if (room == null || depth < 0) return;

        // Adiciona a sala atual à lista de visíveis
        collected.Add(room);

        if (depth > 0)
        {
            foreach (var neighbor in room.neighbors)
            {
                // Chama recursivamente para os vizinhos, diminuindo a profundidade
                CollectNeighbors(neighbor, depth - 1, collected);
            }
        }
    }
}