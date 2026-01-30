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
    
    // Lista de vizinhos preenchida pelo Gerador
    public List<Room> neighbors = new List<Room>();

    [Header("Spawning")]
    public List<Transform> spawnPoints; 

    // --- OTIMIZAÇÃO VISUAL ---
    private Renderer[] myRenderers;
    private Light[] myLights;
    private Canvas[] myCanvases; // Caso tenha UI na sala (ex: barras de vida)
    private bool isVisible = true;

    void Awake()
    {
        // Cacheia todos os componentes visuais dos filhos
        // Isso inclui paredes, chão, decorações, etc.
        myRenderers = GetComponentsInChildren<Renderer>();
        myLights = GetComponentsInChildren<Light>();
        myCanvases = GetComponentsInChildren<Canvas>();
    }

    public void ToggleVisibility(bool state)
    {
        // Evita processamento desnecessário se já estiver no estado correto
        if (isVisible == state) return;

        isVisible = state;

        // Liga/Desliga renderizadores (O objeto continua existindo na física)
        foreach (var r in myRenderers) if(r != null) r.enabled = state;
        
        // Liga/Desliga luzes
        foreach (var l in myLights) if(l != null) l.enabled = state;
        
        // Liga/Desliga UI World Space (se houver)
        foreach (var c in myCanvases) if(c != null) c.enabled = state;

        // Opcional: Se você quiser desligar os Inimigos completamente (para economizar CPU)
        // você pode acessar o RoomManager e pausá-los, mas manter a sala ativa.
    }

    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var sp in spawnPoints) if(sp != null) Gizmos.DrawSphere(sp.position, 0.5f);
        }
    }
}