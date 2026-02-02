// Copyright (c) 2025 Krishnamohan Yagneswaran
// Purchased from Itch.io

using UnityEngine;
using TMPro;

namespace JacksonJames
{
    public class SimpleDialogueNoNames : MonoBehaviour
    {
        [TextArea(2, 5)]
        public string[] dialogueLines;

        public TextMeshProUGUI dialogueText;

        private int currentLineIndex = 0;

        void Start()
        {
            ShowLine();
        }

        void OnInteract()
        {
            currentLineIndex++;
            if (currentLineIndex < dialogueLines.Length)
            {
                ShowLine();
            }
            else
            {
                dialogueText.text = ""; // Clear text or handle end of dialogue
                Debug.Log("End of dialogue.");
            }
        }

        void ShowLine()
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }

        void OnEnable()
        {
            Manager_Events.Input.OnInteract += OnInteract;
        }

        void OnDisable()
        {
            Manager_Events.Input.OnInteract -= OnInteract;
        }

    }
}
