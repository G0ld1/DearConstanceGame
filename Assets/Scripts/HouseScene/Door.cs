using System;
using UnityEngine;
using Yarn.Unity;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private bool isOpen = false;
    
    [Header("Animation Settings")]
    [SerializeField] private float openAngle = 90f; // Ângulo para abrir a porta
    [SerializeField] private float animationSpeed = 2f; // Velocidade da animação
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

       [Header("Narrative System")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string DialogueName;
    
    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
 
    [SerializeField] private AudioSource audioSource;
    
    private Vector3 closedRotation;
    private Vector3 openRotation;
    private bool isAnimating = false;
    private bool playerInRange = false;

    private void Start()
    {
        // Store initial rotation as closed position
        closedRotation = transform.eulerAngles;
        openRotation = closedRotation + new Vector3(0, openAngle, 0);
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Handle interaction input
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isAnimating)
        {
            TryInteract();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
       
        }
    }

    private void TryInteract()
    {
        if (!isInteractable)
        {
            Debug.Log("Door is not interactable");
            return;
        }

        if (DialogueName != String.Empty && isInteractable)
        {
            dialogueRunner.StartDialogue(DialogueName);
        }

     

        if (isInteractable && this.gameObject.name == "ConstanceDoor")
            {
                Debug.Log("è a ultima porta, a dar gaming");
                GameManager.Instance.OnLetterDelivered();
            }

        if (isLocked)
        {
            PlayLockedFeedback();
            return;
        }
        
        // Toggle door state
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;

        isOpen = true;
        
        // Use SoundManager instead of local audio
        if (openSound != null)
            SoundManager.Instance.PlaySFX(openSound);
        
        StartCoroutine(AnimateDoor(openRotation));
    }

    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;

        isOpen = false;
        
        // Use SoundManager instead of local audio
        if (closeSound != null)
            SoundManager.Instance.PlaySFX(closeSound);
        
        StartCoroutine(AnimateDoor(closedRotation));
    }

    private System.Collections.IEnumerator AnimateDoor(Vector3 targetRotation)
    {
        isAnimating = true;
        
        Vector3 startRotation = transform.eulerAngles;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * animationSpeed;
            float normalizedTime = Mathf.Clamp01(elapsedTime);
            float curveValue = openCurve.Evaluate(normalizedTime);
            
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, curveValue);
            transform.eulerAngles = currentRotation;
            
            yield return null;
        }

        transform.eulerAngles = targetRotation;
        isAnimating = false;
    }

    private void PlayLockedFeedback()
    {
        Debug.Log("Door is locked!");


        if (dialogueRunner != null)
        {
            dialogueRunner.StartDialogue("DoorLocked");
        }
      
    }

   

    // Public methods for GameManager
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        
       
        
        Debug.Log($"Door {gameObject.name} is now {(locked ? "locked" : "unlocked")}");
    }

    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
    }

    public bool CanOpen()
    {
        return !isLocked && isInteractable;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    private void ShowInteractionPrompt()
    {
        if (!isInteractable) return;
        
        string promptText = isLocked ? "Door is locked" : 
                           isOpen ? "Press E to close" : "Press E to open";
        
        Debug.Log(promptText); // Replace with UI prompt
        
        // TODO: Show UI prompt
    }

    // Optional: Auto-close after delay
    public void AutoClose(float delay)
    {
        if (isOpen)
        {
            Invoke(nameof(CloseDoor), delay);
        }
    }
}
