using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public float moveSpeed = 5f;
    public float moveDistance = 3f;
    
    [Header("Auto-Open Settings")]
    public float detectionRange = 5.0f; // Distância para ABRIR
    public float hysteresisBuffer = 1.0f; // Distância extra necessária para FECHAR
    public bool isLocked = false;       

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Transform playerTransform;
    private bool shouldBeOpen = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = transform.position + (Vector3.up * moveDistance);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        transform.position = closedPosition;
    }

    void Update()
    {
        // 1. Prioridade Absoluta: Combate (Trava Mestra)
        if (isLocked)
        {
            MoveDoor(closedPosition);
            return; 
        }

        // 2. Lógica de Proximidade com Histerese
        if (playerTransform != null)
        {
            // CORREÇÃO CRÍTICA: Mede a distância até a posição FECHADA (Fixa no mundo)
            // e não até a porta que está se mexendo (transform.position)
            float dist = Vector3.Distance(closedPosition, playerTransform.position);

            if (shouldBeOpen)
            {
                // Se já está aberta, só fecha se o jogador se afastar MUITO (Range + Buffer)
                if (dist > detectionRange + hysteresisBuffer)
                {
                    shouldBeOpen = false;
                }
            }
            else
            {
                // Se está fechada, abre assim que entrar no Range
                if (dist < detectionRange)
                {
                    shouldBeOpen = true;
                }
            }
        }

        // 3. Movimento Suave
        Vector3 target = shouldBeOpen ? openPosition : closedPosition;
        MoveDoor(target);
    }

    void MoveDoor(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }
    
    public void LockDoor() 
    {
        isLocked = true;
        shouldBeOpen = false; // Força o estado lógico a fechar também
    }

    public void UnlockDoor() 
    {
        isLocked = false;
    }
}