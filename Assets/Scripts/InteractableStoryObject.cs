using UnityEngine;
using Yarn.Unity;

public class InteractableStoryObject : MonoBehaviour
{
    [SerializeField] private string storyNodeName;
    [SerializeField] private DialogueRunner dialogueRunner;
    private bool canInteract;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            // Mostrar prompt de interação
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            // Esconder prompt de interação
        }
    }

    private void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E) && !dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue(storyNodeName);
        }
    }
}