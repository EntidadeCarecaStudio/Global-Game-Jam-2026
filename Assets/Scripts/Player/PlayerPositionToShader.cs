using UnityEngine;

public class PlayerPositionToShader : MonoBehaviour
{
    private Transform playerTransform;
    public string playerTag = "";

    void Update()
    {
        // Se ainda não encontramos o player, tentamos achar agora
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        // Se o player já foi encontrado (ou acabou de ser), envia a posição
        if (playerTransform != null)
        {
            Shader.SetGlobalVector("_VectorPlayerPos", playerTransform.position);
        }
    }
}