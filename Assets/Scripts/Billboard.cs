using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _mainCameraTransform;

    void Start()
    {
        // Pega a referência da câmera principal (Cinemachine usa a MainCamera)
        if (Camera.main != null)
            _mainCameraTransform = Camera.main.transform;
    }

    // Usamos LateUpdate para garantir que a câmera já tenha se movido 
    // antes de rotacionarmos o sprite
    void LateUpdate()
    {
        if (_mainCameraTransform == null) return;

        // Opção A: Olhar diretamente para a câmera (pode inclinar se a câmera estiver alta)
        //transform.LookAt(_mainCameraTransform);

        // Opção B: Rotação Cilíndrica (Mais comum em jogos 2.5D)
        // O sprite rotaciona apenas no eixo Y, ficando sempre reto.
        Vector3 targetPosition = _mainCameraTransform.position;
        targetPosition.y = transform.position.y; // Mantém o sprite "em pé"
        transform.LookAt(targetPosition);

        // Ajuste de inversão: Dependendo de como seu sprite foi desenhado, 
        // ele pode aparecer de costas. Se isso ocorrer, descomente a linha abaixo:
        transform.Rotate(0, 180, 0);
    }
}