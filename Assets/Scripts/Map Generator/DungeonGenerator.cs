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

    [Header("Generation Rules")]
    public int desiredRooms = 20;
    public int maxSimulationAttempts = 100;
    
    [Header("Collision Settings")]
    [Tooltip("Minimum distance required between room centers to avoid overlap.")]
    public float roomRadius = 8f; 
    // Kept grid for alignment, but collision is now distance-based too
    public float gridSize = 10f; 

    // --- VIRTUAL DATA STRUCTURES ---
    private class VirtualRoom
    {
        public Room prefabReference;
        public Vector3 position;
        public Quaternion rotation;
        public Room.RoomType type;
        
        // Data for Logic-Based Wall Generation
        public List<Vector3> connectorsPoints;
        public List<Quaternion> connectorsRots;
        public bool[] isConnectorUsed; // Tracks which specific connector is open/closed
    }

    // Cache to avoid instantiating prefabs constantly
    private Dictionary<Room, RoomDataCache> prefabCache = new Dictionary<Room, RoomDataCache>();
    private struct RoomDataCache 
    { 
        public List<TransformData> connectors; 
        public int connectorCount;
    }
    private struct TransformData { public Vector3 localPos; public Quaternion localRot; }

    void Start()
    {
        CachePrefabData(startRoomPrefab);
        CachePrefabData(bossRoomPrefab);
        foreach (var r in roomPrefabs) CachePrefabData(r);
        foreach (var c in corridorPrefabs) CachePrefabData(c);

        GenerateDungeon();
    }

    void CachePrefabData(Room prefab)
    {
        if(prefabCache.ContainsKey(prefab)) return;

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
        
        // Setup Start Room
        VirtualRoom vStart = CreateVirtualRoom(startRoomPrefab, Vector3.zero, Quaternion.identity);
        virtualRooms.Add(vStart);

        // Track available connectors: (RoomIndex, ConnectorIndex)
        List<(int roomIdx, int connIdx)> pendingConnectors = new List<(int, int)>();
        
        // Add all start connectors to pending
        for(int i=0; i < vStart.connectorsPoints.Count; i++) 
            pendingConnectors.Add((0, i));

        int roomsCreated = 1;
        int safety = 0;

        while (roomsCreated < desiredRooms && pendingConnectors.Count > 0 && safety < 500)
        {
            safety++;

            // Pick random connector
            int pendingIndex = Random.Range(0, pendingConnectors.Count);
            var (parentIdx, parentConnIdx) = pendingConnectors[pendingIndex];
            VirtualRoom parentRoom = virtualRooms[parentIdx];

            // Select candidate
            Room[] candidates = (parentRoom.type == Room.RoomType.Corridor) ? roomPrefabs : corridorPrefabs;
            if (parentRoom.type == Room.RoomType.Start || parentRoom.type == Room.RoomType.Room) candidates = corridorPrefabs;
            
            Room prefabToTry = candidates[Random.Range(0, candidates.Length)];
            RoomDataCache entryData = prefabCache[prefabToTry];

            // Calculate Transform
            // Note: We always enter via connector 0 of the new room
            TransformData childEntry = entryData.connectors[0]; 
            Vector3 targetPos = parentRoom.connectorsPoints[parentConnIdx];
            Quaternion targetRot = parentRoom.connectorsRots[parentConnIdx];

            Quaternion requiredRot = (targetRot * Quaternion.Euler(0, 180, 0)) * Quaternion.Inverse(childEntry.localRot);
            Vector3 requiredPos = targetPos - (requiredRot * childEntry.localPos);

            // --- IMPROVED COLLISION CHECK (Radius + Bounds) ---
            if (IsPositionValid(requiredPos, virtualRooms))
            {
                VirtualRoom vNew = CreateVirtualRoom(prefabToTry, requiredPos, requiredRot);
                virtualRooms.Add(vNew);
                roomsCreated++;
                int newRoomIdx = virtualRooms.Count - 1;

                // --- LOGIC CONNECTION MARKING ---
                // Mark Parent Connector as Used
                parentRoom.isConnectorUsed[parentConnIdx] = true;
                // Mark Child Entry (0) as Used
                vNew.isConnectorUsed[0] = true;

                // Add new room's OTHER connectors to pending
                for (int i = 1; i < vNew.connectorsPoints.Count; i++)
                {
                    pendingConnectors.Add((newRoomIdx, i));
                }

                // Remove used connector from pending
                pendingConnectors.RemoveAt(pendingIndex);
            }
        }

        // --- BOSS PLACEMENT ---
        if (roomsCreated >= desiredRooms)
        {
            // Find furthest connector
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
                    
                    // Mark connections
                    pRoom.isConnectorUsed[cIdx] = true;
                    vBoss.isConnectorUsed[0] = true;

                    virtualRooms.Add(vBoss);
                    return virtualRooms; // Success
                }
            }
        }

        return null; // Failed
    }

    bool IsPositionValid(Vector3 pos, List<VirtualRoom> existingRooms)
    {
        // 1. Grid Snap Check (optional, keeps things aligned)
        // Vector2Int grid = new Vector2Int(Mathf.RoundToInt(pos.x / gridSize), Mathf.RoundToInt(pos.z / gridSize));
        // if (occupiedGrid.Contains(grid)) return false; 
        
        // 2. Radius Check (More robust for various room sizes)
        foreach(var room in existingRooms)
        {
            if(Vector3.Distance(pos, room.position) < roomRadius)
            {
                return false;
            }
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
        vr.connectorsPoints = new List<Vector3>();
        vr.connectorsRots = new List<Quaternion>();

        var cachedData = prefabCache[prefab];
        
        // Init Logic-Based Connector Tracker
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
        foreach (var vr in layout)
        {
            Room newRoom = Instantiate(vr.prefabReference, vr.position, vr.rotation, transform);
            
            // --- LOGIC-BASED WALL GENERATION ---
            // No Physics! We rely on the bool array we populated during simulation.
            for(int i=0; i < vr.isConnectorUsed.Length; i++)
            {
                // If connector is NOT used, plug it with a wall
                if (!vr.isConnectorUsed[i])
                {
                    if (wallPrefab != null)
                    {
                        Transform connectorTrans = newRoom.connectors[i];

                        // Modified line: Adds +2 to the Y axis
                        Vector3 wallPos = connectorTrans.position + (Vector3.up * 2f);

                        Instantiate(wallPrefab, wallPos, connectorTrans.rotation, transform);
                    }
                }
            }
        }
    }
}