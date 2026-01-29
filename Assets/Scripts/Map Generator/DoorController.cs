using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen = true; // Começa aberta para o jogador poder entrar
    public float moveSpeed = 5f;
    public float moveDistance = 3f; // Quanto ela sobe

    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = transform.position + (Vector3.up * moveDistance);
        
        // Se começar aberta, já posiciona lá em cima
        if (isOpen) transform.position = openPosition;
    }

    void Update()
    {
        // Define o alvo baseado no estado
        Vector3 target = isOpen ? openPosition : closedPosition;

        // Move suavemente para o alvo
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }
    
    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }
}