using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    public enum RoomType { Room, Corridor, Boss, Start }
    public RoomType type;
    public List<Transform> connectors;
}
