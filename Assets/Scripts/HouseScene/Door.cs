using UnityEngine;

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
    
    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;
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
            HideInteractionPrompt();
        }
    }

    private void TryInteract()
    {
        if (!isInteractable)
        {
            Debug.Log("Door is not interactable");
            return;
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
        StartCoroutine(AnimateDoor(openRotation, openSound));
    }

    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;

        isOpen = false;
        StartCoroutine(AnimateDoor(closedRotation, closeSound));
    }

    private System.Collections.IEnumerator AnimateDoor(Vector3 targetRotation, AudioClip sound)
    {
        isAnimating = true;
        
        Vector3 startRotation = transform.eulerAngles;
        float elapsedTime = 0f;
        
        // Play sound
        if (sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sound);
        }

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * animationSpeed;
            float normalizedTime = Mathf.Clamp01(elapsedTime);
            float curveValue = openCurve.Evaluate(normalizedTime);
            
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, curveValue);
            transform.eulerAngles = currentRotation;
            
            yield return null;
        }

        // Ensure final position is exact
        transform.eulerAngles = targetRotation;
        isAnimating = false;
    }

    private void PlayLockedFeedback()
    {
        Debug.Log("Door is locked!");
        
        if (lockedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lockedSound);
        }

        // Optional: shake effect or visual feedback
        StartCoroutine(ShakeDoor());
    }

    private System.Collections.IEnumerator ShakeDoor()
    {
        Vector3 originalPosition = transform.position;
        float shakeAmount = 0.1f;
        float shakeDuration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            Vector3 randomPoint = originalPosition + Random.insideUnitSphere * shakeAmount;
            randomPoint.y = originalPosition.y; // Keep Y position constant
            transform.position = randomPoint;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    // Public methods for GameManager
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        
        // Visual feedback (optional)
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.color = locked ? Color.red : Color.green;
        }
        
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

    private void HideInteractionPrompt()
    {
        // TODO: Hide UI prompt
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
