using UnityEngine;
using Yarn.Unity;

public class RippleOverlayEffect : MonoBehaviour
{
    [Header("Overlay Settings")]
    [SerializeField] private Material rippleOverlayMaterial;
    [SerializeField] private bool enableEffect = true;
    [SerializeField] private float maxDistance = 12f; // Aumentei para ser visível de mais longe
    [SerializeField] private Color rippleColor = new Color(0.2f, 0.6f, 1f, 0.5f);

    [Header("Ripple Properties")]
    [SerializeField] private float rippleSpeed = 2.5f;     // Velocidade constante
    [SerializeField] private float rippleSize = 6f;       // Tamanho das ondas
    [SerializeField] private float rippleIntensity = 0.3f; // Intensidade máxima
    [SerializeField] private float fadeSpeed = 2f;        // Velocidade de fade out

    private Material overlayInstance;
    private Renderer objectRenderer;
    private Camera playerCamera;
    private Material[] originalMaterials;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        playerCamera = Camera.main;

        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();

        SetupOverlay();
    }

    private void SetupOverlay()
    {
        if (objectRenderer == null || rippleOverlayMaterial == null) return;

        // Store original materials
        originalMaterials = objectRenderer.materials;

        // Create overlay material instance
        overlayInstance = new Material(rippleOverlayMaterial);
        overlayInstance.SetColor("_RippleColor", rippleColor);

        // Add overlay as additional material (renders on top)
        Material[] newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[newMaterials.Length - 1] = overlayInstance;

        objectRenderer.materials = newMaterials;

        Debug.Log($"Added ripple overlay to {gameObject.name}");
    }

    private void Update()
    {
        if (!enableEffect || overlayInstance == null || playerCamera == null) 
            return;

        bool canInteract = CanInteract();
        float distance = Vector3.Distance(transform.position, playerCamera.transform.position);
        
        if (canInteract && distance <= maxDistance)
        {
            // Fixed ripple center - sempre no centro do objeto
            Vector2 rippleCenter = new Vector2(0.5f, 0.5f);
            
            // Apply FIXED settings - nada muda
            overlayInstance.SetVector("_RippleCenter", rippleCenter);
            overlayInstance.SetFloat("_RippleSpeed", rippleSpeed);
            overlayInstance.SetFloat("_RippleSize", rippleSize);
            overlayInstance.SetFloat("_RippleRadius", 1.2f);
            overlayInstance.SetFloat("_RippleIntensity", rippleIntensity);
        }
        else
        {
            // Turn off effect
            overlayInstance.SetFloat("_RippleIntensity", 0f);
        }
        
        // Debug toggle
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleOverlay();
        }
    }

    private bool CanInteract()
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.CanInteract();
        }
        return gameObject.activeInHierarchy && GetComponent<InteractableStoryObject>() != null;
    }

    private void ToggleOverlay()
    {
        if (objectRenderer != null)
        {
            if (objectRenderer.materials.Length > originalMaterials.Length)
            {
                // Remove overlay
                objectRenderer.materials = originalMaterials;
                Debug.Log("Overlay removed");
            }
            else
            {
                // Add overlay back
                SetupOverlay();
                Debug.Log("Overlay added");
            }
        }
    }

    public void EnableOverlay(bool enable)
    {
        enableEffect = enable;
        if (!enable && objectRenderer != null)
        {
            objectRenderer.materials = originalMaterials;
        }
        else if (enable)
        {
            SetupOverlay();
        }
    }

    public void DisableRipple()
    {
        enableEffect = false;
        if (overlayInstance != null)
        {
            overlayInstance.SetFloat("_RippleIntensity", 0f);
        }
        Debug.Log($"Ripple effect disabled on {gameObject.name}");
    }

    private void OnDestroy()
    {
        if (overlayInstance != null)
        {
            DestroyImmediate(overlayInstance);
        }
    }

    [YarnCommand("disable_ripple")]
    public static void DisableRippleCommand(string objectName)
    {
        // Encontrar o objeto pelo nome
        GameObject targetObject = GameObject.Find(objectName);
        
        if (targetObject != null)
        {
            var rippleEffect = targetObject.GetComponent<RippleOverlayEffect>();
            if (rippleEffect != null)
            {
                rippleEffect.DisableRipple();
                Debug.Log($"Ripple effect disabled for {objectName}");
            }
            else
            {
                Debug.LogWarning($"No RippleOverlayEffect found on {objectName}");
            }
        }
        else
        {
            Debug.LogWarning($"GameObject '{objectName}' not found");
        }
    }
}
