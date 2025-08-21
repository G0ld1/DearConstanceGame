using UnityEngine;
using System;

public class InteractableObject : MonoBehaviour
{
    [Header("Identification")]
    [SerializeField] private string objectId;
    [SerializeField] private string displayName;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask playerLayer = 1;
    [SerializeField] private bool requiresDirectLook = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private Canvas interactionUI;
    [SerializeField] private TMPro.TextMeshProUGUI interactionText;
    
    [Header("Audio")]
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private AudioSource audioSource;
    
    private Camera playerCamera;
    private bool canInteract = false;
    private bool isHighlighted = false;
    private bool isPlayerInRange = false;
    
    // Eventos
    public static event Action<string> OnObjectInteracted;
    public static event Action<string> OnObjectHighlighted;
    public static event Action<string> OnObjectUnhighlighted;
    
    public string ObjectId => objectId;
    public string DisplayName => displayName;

    private void Start()
    {
        playerCamera = Camera.main;
        if (interactionText != null)
        {
            interactionText.text = $"Clique para interagir com {displayName}";
        }
        
        // Começa desativado
        DisableInteraction();
    }

    private void Update()
    {
        if (canInteract)
        {
            CheckPlayerProximity();
            
            if (isPlayerInRange)
            {
                if (requiresDirectLook)
                {
                    CheckPlayerLook();
                }
                else
                {
                    ShowInteractionUI();
                }
                
                CheckForInput();
            }
            else
            {
                HideInteractionUI();
            }
        }
    }

    private void CheckPlayerProximity()
    {
        if (playerCamera == null) return;
        
        float distance = Vector3.Distance(transform.position, playerCamera.transform.position);
        bool inRange = distance <= interactionDistance;
        
        if (inRange != isPlayerInRange)
        {
            isPlayerInRange = inRange;
            Debug.Log("Player esta no range, pode interagir");
            
            if (!isPlayerInRange)
            {
                HideInteractionUI();
            }
        }
    }

    private void CheckPlayerLook()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        // Debug mais detalhado
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 0.1f);
        
        bool hitSomething = Physics.Raycast(ray, out hit, interactionDistance);
        
        if (hitSomething)
        {
            Debug.Log($"Raycast hit: {hit.collider.name} | This object: {gameObject.name}");
            Debug.Log($"Hit distance: {hit.distance} | Max distance: {interactionDistance}");
        }
        else
        {
            Debug.Log("Raycast não acertou nada");
        }
        
        bool isLookingAt = hitSomething && hit.collider.gameObject == gameObject;
        
        Debug.Log($"Is looking at {gameObject.name}: {isLookingAt}");
        
        if (isLookingAt && !isHighlighted)
        {
            ShowInteractionUI();
        }
        else if (!isLookingAt && isHighlighted)
        {
            HideInteractionUI();
        }
    }

    private void ShowInteractionUI()
    {
        if (!isHighlighted)
        {
           
            isHighlighted = true;
             Debug.Log("OBjeto higlighted");
            if (highlightEffect != null)
                highlightEffect.SetActive(true);
                
            if (interactionUI != null)
                interactionUI.gameObject.SetActive(true);
            
            OnObjectHighlighted?.Invoke(objectId);
        }
    }

    private void HideInteractionUI()
    {
        if (isHighlighted)
        {
            isHighlighted = false;
            
            if (highlightEffect != null)
                highlightEffect.SetActive(false);
                
            if (interactionUI != null)
                interactionUI.gameObject.SetActive(false);
            
            OnObjectUnhighlighted?.Invoke(objectId);
        }
    }

    private void CheckForInput()
    {
       
        if (isHighlighted && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player interagiu");
            InteractWithObject();
        }
    }

    private void InteractWithObject()
    {
        // Play sound
        if (interactionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
        
        // Hide UI
        HideInteractionUI();
        
        // Notify system
        OnObjectInteracted?.Invoke(objectId);
        
        Debug.Log($"Interacted with {displayName} (ID: {objectId})");
    }

    public void EnableInteraction()
    {
        canInteract = true;
        Debug.Log($"Interaction enabled for {displayName}");
    }

    public void DisableInteraction()
    {
        canInteract = false;
        HideInteractionUI();
        Debug.Log($"Interaction disabled for {displayName}");
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha a área de interação no editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
