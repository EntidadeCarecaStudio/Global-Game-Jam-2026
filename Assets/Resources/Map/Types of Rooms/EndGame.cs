using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public string level;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se é o jogador e se a sala é um MiniBoss
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadSceneAsync(level);
        }
    }

}
