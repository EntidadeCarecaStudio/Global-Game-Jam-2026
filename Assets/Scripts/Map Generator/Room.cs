using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    // Adicionado o tipo MiniBoss
    public enum RoomType { Room, Corridor, Boss, Start, MiniBoss }
    public RoomType type;

    // Novo enum para definir o elemento (importante para os Minibosses)
    public enum RoomElement { None, Fire, Ice, Earth }
    public RoomElement element; // Defina isso no Inspector dos prefabs de Miniboss
    
    [Header("Connections")]
    public List<Transform> connectors;

    [Header("Spawning")]
    public List<Transform> spawnPoints; 

    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var sp in spawnPoints)
            {
                if(sp != null) Gizmos.DrawSphere(sp.position, 0.5f);
            }
        }
    }
}
