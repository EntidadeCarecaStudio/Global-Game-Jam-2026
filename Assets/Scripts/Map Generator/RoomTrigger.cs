using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    private Room room;
    private bool dialogueTriggered = false;

    void Awake()
    {
        room = GetComponent<Room>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se é o jogador e se a sala é um MiniBoss
        if (other.CompareTag("Player") && room.type == Room.RoomType.MiniBoss && !dialogueTriggered)
        {
            TriggerBossDialogue();
            dialogueTriggered = true; 
        }
    }

    private void TriggerBossDialogue()
    {
        SceneDialogueManager dialogueManager = FindFirstObjectByType<SceneDialogueManager>();
        
        if (dialogueManager == null) return;

        // Mapeia o Elemento para o índice da lista conforme solicitado
        int index = -1;

        switch (room.element)
        {
            case Room.RoomElement.Fire:
                index = 0; // Elemento 0: Fire
                break;
            case Room.RoomElement.Earth:
                index = 1; // Elemento 1: Earth
                break;
            case Room.RoomElement.Ice:
                index = 2; // Elemento 2: Ice
                break;
        }

        if (index != -1)
        {
            dialogueManager.TriggerSpecificDialogue(index);
        }
    }
}