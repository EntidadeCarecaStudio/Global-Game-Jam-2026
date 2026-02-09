using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDialogueManager : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueEntry
    {
        public string label; 
        [TextArea(3, 10)]
        public string dialogueText;
        public string characterID;
    }

    [Header("Configuração de Falas")]
    [SerializeField] private List<DialogueEntry> dialogueList = new List<DialogueEntry>();

    [Header("Modo de Operação")]
    [Tooltip("Toca a lista em ordem (Cutscene). Desativa 'Falas Específicas'.")]
    public bool dialogoSequencial = true;
    
    [Tooltip("Permite disparar falas por índice (Gameplay). Desativa 'Diálogo Sequencial'.")]
    public bool falasEspecificas = false;

    [Header("Estado")]
    public bool allDialoguesFinished = false;

    // --- LOGICA DE EXCLUSIVIDADE NO EDITOR ---
    private void OnValidate()
    {
        // Se você marcar Sequencial, desmarca Específica
        if (dialogoSequencial && falasEspecificas)
        {
            falasEspecificas = false;
        }
    }

    private void Start()
    {
        // Só toca automaticamente se estiver no modo Sequencial
        if (dialogoSequencial)
        {
            TriggerFullSequence();
        }
    }

    // Método para Cutscenes (Toca tudo em ordem)
    public void TriggerFullSequence()
    {
        if (dialogueList.Count == 0) return;

        allDialoguesFinished = false;
        foreach (var entry in dialogueList)
        {
            SendToController(entry);
        }
        StartCoroutine(CheckDialogueEnd());
    }

    // Método para Gameplay (Ex: Chamar fala de um Miniboss específico)
    public void TriggerSpecificDialogue(int index)
    {
        if (!falasEspecificas)
        {
            Debug.LogWarning("Tentando chamar fala específica, mas o modo 'Falas Específicas' não está ativo!");
            return;
        }

        if (index >= 0 && index < dialogueList.Count)
        {
            allDialoguesFinished = false;
            SendToController(dialogueList[index]);
            StartCoroutine(CheckDialogueEnd());
        }
    }

    private void SendToController(DialogueEntry entry)
    {
        if (string.IsNullOrEmpty(entry.characterID))
            DialogueController.instance.NewDialogueInstance(entry.dialogueText);
        else
            DialogueController.instance.NewDialogueInstance(entry.dialogueText, entry.characterID);
    }

    private IEnumerator CheckDialogueEnd()
    {
        // Espera o Controller começar a processar
        yield return new WaitUntil(() => DialogueController.instance.IsDialogueActive);
        // Espera o Controller terminar tudo
        yield return new WaitWhile(() => DialogueController.instance.IsDialogueActive);
        
        allDialoguesFinished = true;
    }
}