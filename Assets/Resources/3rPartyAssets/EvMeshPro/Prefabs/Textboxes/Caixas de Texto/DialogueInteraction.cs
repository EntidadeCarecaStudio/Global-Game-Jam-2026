using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueInteraction : MonoBehaviour
{
    public string firstLevel;
    private SceneDialogueManager sceneDialogueManager;
    private bool isChangingScene = false; // Flag para evitar chamadas duplicadas

    private void Start() 
    {
        // Encontra o gerenciador no início do jogo
        sceneDialogueManager = Object.FindAnyObjectByType<SceneDialogueManager>();

        if (sceneDialogueManager == null)
        {
            Debug.LogError("SceneDialogueManager não foi encontrado na cena!");
        }
    }

    private void Update() 
    {
        // Só executa se o gerenciador existir e a cena ainda não estiver mudando
        if (sceneDialogueManager != null && !isChangingScene)
        {
            if (sceneDialogueManager.allDialoguesFinished)
            {
                isChangingScene = true; // Garante que ChangeScene seja chamada apenas uma vez
                ChangeScene(firstLevel);
            }
        }
    }

    public void ChangeScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("O nome da cena está vazio!");
        }
    }
}