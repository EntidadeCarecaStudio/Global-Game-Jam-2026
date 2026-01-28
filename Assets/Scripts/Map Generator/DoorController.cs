using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen = false; // Começa falso como solicitado
    public float liftSpeed = 2f;
    public float destroyHeight = 15f;

    void Update()
    {
        // Se o bool for verdadeiro, inicia a "animação"
        if (isOpen)
        {
            // Move para cima no eixo Y
            transform.Translate(Vector3.up * liftSpeed * Time.deltaTime);

            // Verifica se atingiu a altura para destruir
            if (transform.position.y >= destroyHeight)
            {
                gameObject.SetActive(false); // Desativa o objeto
            }
        }
    }
    
    // Método público para ser chamado por gatilhos ou eventos do jogo
    public void OpenDoor()
    {
        isOpen = true;
    }
}
