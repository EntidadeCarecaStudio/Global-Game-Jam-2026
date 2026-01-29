using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    public enum RoomType { Room, Corridor, Boss, Start, MiniBoss }
    public RoomType type;
    public enum RoomElement { None, Fire, Ice, Earth }
    public RoomElement element;

    [Header("Connections")]
    public List<Transform> connectors;
    
    // --- NOVO: Lista de vizinhos preenchida pelo Gerador ---
    public List<Room> neighbors = new List<Room>();

    [Header("Spawning")]
    public List<Transform> spawnPoints; 

    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var sp in spawnPoints) if(sp != null) Gizmos.DrawSphere(sp.position, 0.5f);
        }
    }
}