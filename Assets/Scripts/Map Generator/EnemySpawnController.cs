using UnityEngine;
using System.Collections;
using UnityEngine.Events; // Para eventos modulares (som, partículas)

public class EnemySpawnController : MonoBehaviour
{
    [Header("Configurações de Spawn")]
    public float spawnDuration = 0.5f; // Tempo para sair do chão
    public float depthOffset = 3.0f;   // O quão fundo ele começa (metros)
    public AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Suavização do movimento

    [Header("Modularidade")]
    public GameObject spawnVFXPrefab; // Arraste partículas de terra/fumaça aqui futuramente

    [Header("AI Context")]
    public Room ownerRoom; // A referência que você vai preencher
    public RoomManager ownerManager; // Opcional, mas útil para avisar que morreu

    private Vector3 finalPosition;
    private Vector3 hiddenPosition;
    private Collider myCollider;
    private MonoBehaviour[] aiScripts; // Para desativar scripts de movimento
    
    private bool hasSpawned = false;

    void Awake()
    {
        // 1. Configuração Inicial: Salva posições e componentes
        myCollider = GetComponent<Collider>();
        
        // Pega scripts comuns de IA (ex: NavMeshAgent ou seus scripts customizados)
        // Ajuste conforme o nome dos seus scripts de IA
        aiScripts = GetComponents<MonoBehaviour>(); 
    }

    // Chamado pelo Gerador ou RoomManager antes do jogador ver
    public void PrepareForSpawn()
    {
        if (hasSpawned) return;

        finalPosition = transform.position + new Vector3(0, 0.6f, 0);
        hiddenPosition = finalPosition - (Vector3.up * depthOffset);

        // Esconde o inimigo no subsolo
        transform.position = hiddenPosition;

        // Desativa colisão e lógica para o jogador não bater em "fantasmas"
        if (myCollider) myCollider.enabled = false;
        
        // Desativa scripts de comportamento (exceto este e o Transform)
        ToggleCombatAI(false);
    }

    public void StartSpawnSequence()
    {
        if (hasSpawned) return;
        StartCoroutine(RiseFromGroundRoutine());
    }

    IEnumerator RiseFromGroundRoutine()
    {
        // Opcional: Instancia efeito visual (poeira/terra)
        if (spawnVFXPrefab != null) 
            Instantiate(spawnVFXPrefab, transform.position, Quaternion.identity);

        float elapsed = 0f;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / spawnDuration;
            
            // Move suavemente de baixo para cima
            transform.position = Vector3.Lerp(hiddenPosition, finalPosition, riseCurve.Evaluate(percent));
            
            yield return null;
        }

        // Garante posição exata no final
        transform.position = finalPosition;
        CompleteSpawn();
    }

    void CompleteSpawn()
    {
        hasSpawned = true;

        // Reativa tudo
        if (myCollider) myCollider.enabled = true;
        ToggleCombatAI(true);
    }

    void ToggleCombatAI(bool state)
    {
        foreach (var script in aiScripts)
        {
            // Não desative este script, nem Transform, nem o RoomManager se estiver no objeto
            if (script != this && script is not RoomManager) 
            {
                script.enabled = state;
            }
        }
        
        // Se usar NavMeshAgent, precisa tratar separado
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = state;
    }

    public bool IsInsideRoomBounds(Vector3 position)
    {
        if (ownerRoom == null) return true; // Sem dono, livre pelo mundo
        
        // Exemplo simples: Distância do centro
        // Os programadores de IA podem melhorar isso usando Colliders depois
        float distance = Vector3.Distance(position, ownerRoom.transform.position);
        return distance < 20f; // Exemplo de raio limite
    }
}