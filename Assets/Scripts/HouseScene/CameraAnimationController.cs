using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class CameraAnimationController : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraParent;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private AnimationCurve defaultEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Look Targets")]
    [SerializeField] private Transform[] predefinedLookTargets;
    
    // Estado original da câmera
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Vector3 originalParentRotation;
    
    // Controle de animação
    private bool isAnimating = false;
    private bool playerControlsDisabled = false;
    
    // Referência ao movimento do jogador
    private PlayerMovementScript playerMovement;
    
    // Instância singleton para métodos static
    public static CameraAnimationController Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple CameraAnimationController instances found!");
        }
    }
    
    void Start()
    {
        // Guarda as posições/rotações originais
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (cameraParent == null)
            cameraParent = playerCamera.transform.parent;
            
        StoreOriginalTransforms();
        
        // Encontra o script de movimento do jogador
        playerMovement = FindFirstObjectByType<PlayerMovementScript>();
    }
    
    private void StoreOriginalTransforms()
    {
        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.localPosition;
            originalCameraRotation = playerCamera.transform.localRotation;
        }
        
        if (cameraParent != null)
        {
            originalParentRotation = cameraParent.localEulerAngles;
        }
    }
    
    // === MÉTODOS YARN COMMAND STATIC ===
    
    [YarnCommand("camera_look_at")]
    public static void LookAtTarget(string targetName, float duration = 2f)
    {
        if (Instance != null)
        {
            Instance.LookAtTargetInstance(targetName, duration);
        }
        else
        {
            Debug.LogError("CameraAnimationController instance not found!");
        }
    }
    
    [YarnCommand("camera_nod")]
    public static void NodHead(float intensity = 1f, float duration = 1f)
    {
        if (Instance != null)
        {
            Instance.NodHeadInstance(intensity, duration);
        }
        else
        {
            Debug.LogError("CameraAnimationController instance not found!");
        }
    }
    
    [YarnCommand("camera_shake_head")]
    public static void ShakeHead(float intensity = 1f, float duration = 1f)
    {
        if (Instance != null)
        {
            Instance.ShakeHeadInstance(intensity, duration);
        }
        else
        {
            Debug.LogError("CameraAnimationController instance not found!");
        }
    }
    
    [YarnCommand("camera_tilt")]
    public static void TiltHead(float angle = 15f, float duration = 1f)
    {
        if (Instance != null)
        {
            Instance.TiltHeadInstance(angle, duration);
        }
        else
        {
            Debug.LogError("CameraAnimationController instance not found!");
        }
    }
    
    [YarnCommand("camera_look_down")]
    public static void LookDown(float angle = 30f, float duration = 1.5f)
    {
        if (Instance != null)
        {
            Instance.LookVerticalInstance(-angle, duration);
        }
        else
        {
            Debug.LogError("CameraAnimationController instance not found!");
        }
    }
    
    [YarnCommand("camera_look_up")]
    public static void LookUp(float angle = 20f, float duration = 1.5f)
    {
        if (Instance != null)
        {
            Instance.LookVerticalInstance(angle, duration);
        }
        else
        {
            Debug.LogError("CameraAnimationController instance not found!");
        }
    }
    
    [YarnCommand("camera_reset")]
    public static void ResetCameraPosition(float duration = 1f)
    {
        if (Instance != null)
        {
            Instance.ResetCameraInstance(duration);
        }
        else
        {
            Debug.LogError("CameraAnimationController instance not found!");
        }
    }
    
    // === MÉTODOS DE INSTÂNCIA (IMPLEMENTAÇÃO REAL) ===
    
    public void LookAtTargetInstance(string targetName, float duration = 2f)
    {
        Transform target = FindLookTarget(targetName);
        if (target != null)
        {
            StartCoroutine(LookAtTargetCoroutine(target, duration));
        }
        else
        {
            Debug.LogWarning($"Look target '{targetName}' not found!");
        }
    }
    
    public void NodHeadInstance(float intensity = 1f, float duration = 1f)
    {
        StartCoroutine(NodHeadCoroutine(intensity, duration));
    }
    
    public void ShakeHeadInstance(float intensity = 1f, float duration = 1f)
    {
        StartCoroutine(ShakeHeadCoroutine(intensity, duration));
    }
    
    public void TiltHeadInstance(float angle = 15f, float duration = 1f)
    {
        StartCoroutine(TiltHeadCoroutine(angle, duration));
    }
    
    public void LookVerticalInstance(float angle, float duration)
    {
        StartCoroutine(LookVerticalCoroutine(angle, duration));
    }
    
    public void ResetCameraInstance(float duration = 1f)
    {
        StartCoroutine(ResetCameraCoroutine(duration));
    }
    
    // === CORROTINAS DE ANIMAÇÃO ===
    
    private IEnumerator LookAtTargetCoroutine(Transform target, float duration)
    {
        DisablePlayerControls();
        
        Vector3 startRotation = cameraParent.eulerAngles;
        Vector3 directionToTarget = (target.position - cameraParent.position).normalized;
        Vector3 targetRotation = Quaternion.LookRotation(directionToTarget).eulerAngles;
        
        // Ajusta para rotação local
        targetRotation.x = Mathf.DeltaAngle(0, targetRotation.x);
        targetRotation.y = Mathf.DeltaAngle(0, targetRotation.y);
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = defaultEaseCurve.Evaluate(elapsed / duration);
            
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, t);
            cameraParent.eulerAngles = currentRotation;
            
            yield return null;
        }
        
        EnablePlayerControls();
    }
    
    private IEnumerator NodHeadCoroutine(float intensity, float duration)
    {
        DisablePlayerControls();
        
        Vector3 startRotation = playerCamera.transform.localEulerAngles;
        float nodAngle = 15f * intensity;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = elapsed / duration;
            
            // Movimento de "sim" (para baixo e para cima)
            float currentAngle = Mathf.Sin(t * Mathf.PI * 2f) * nodAngle;
            
            Vector3 rotation = startRotation;
            rotation.x = startRotation.x + currentAngle;
            
            playerCamera.transform.localEulerAngles = rotation;
            
            yield return null;
        }
        
        // Volta à posição original
        playerCamera.transform.localEulerAngles = startRotation;
        EnablePlayerControls();
    }
    
    private IEnumerator ShakeHeadCoroutine(float intensity, float duration)
    {
        DisablePlayerControls();
        
        Vector3 startRotation = playerCamera.transform.localEulerAngles;
        float shakeAngle = 20f * intensity;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = elapsed / duration;
            
            // Movimento de "não" (esquerda e direita)
            float currentAngle = Mathf.Sin(t * Mathf.PI * 3f) * shakeAngle * (1f - t);
            
            Vector3 rotation = startRotation;
            rotation.y = startRotation.y + currentAngle;
            
            playerCamera.transform.localEulerAngles = rotation;
            
            yield return null;
        }
        
        // Volta à posição original
        playerCamera.transform.localEulerAngles = startRotation;
        EnablePlayerControls();
    }
    
    private IEnumerator TiltHeadCoroutine(float angle, float duration)
    {
        DisablePlayerControls();
        
        Vector3 startRotation = playerCamera.transform.localEulerAngles;
        Vector3 targetRotation = startRotation;
        targetRotation.z = angle;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = defaultEaseCurve.Evaluate(elapsed / duration);
            
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, t);
            playerCamera.transform.localEulerAngles = currentRotation;
            
            yield return null;
        }
        
        EnablePlayerControls();
    }
    
    private IEnumerator LookVerticalCoroutine(float angle, float duration)
    {
        DisablePlayerControls();
        
        Vector3 startRotation = playerCamera.transform.localEulerAngles;
        Vector3 targetRotation = startRotation;
        targetRotation.x = startRotation.x + angle;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = defaultEaseCurve.Evaluate(elapsed / duration);
            
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, t);
            playerCamera.transform.localEulerAngles = currentRotation;
            
            yield return null;
        }
        
        EnablePlayerControls();
    }
    
    private IEnumerator ResetCameraCoroutine(float duration)
    {
        DisablePlayerControls();
        
        Vector3 startCameraRotation = playerCamera.transform.localEulerAngles;
        Vector3 startParentRotation = cameraParent.localEulerAngles;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float t = defaultEaseCurve.Evaluate(elapsed / duration);
            
            // Reset camera
            Vector3 currentCameraRotation = Vector3.Lerp(startCameraRotation, originalCameraRotation.eulerAngles, t);
            playerCamera.transform.localEulerAngles = currentCameraRotation;
            
            // Reset parent
            Vector3 currentParentRotation = Vector3.Lerp(startParentRotation, originalParentRotation, t);
            cameraParent.localEulerAngles = currentParentRotation;
            
            yield return null;
        }
        
        EnablePlayerControls();
    }
    
    // === CONTROLE DO JOGADOR ===
    
    private void DisablePlayerControls()
    {
        isAnimating = true;
        playerControlsDisabled = true;
        
        if (playerMovement != null)
        {
            playerMovement.BlockMovement();
        }
    }
    
    private void EnablePlayerControls()
    {
        isAnimating = false;
        playerControlsDisabled = false;
        
        if (playerMovement != null)
        {
            playerMovement.UnblockMovement();
        }
    }
    
    // === UTILITÁRIOS ===
    
    private Transform FindLookTarget(string targetName)
    {
        // Primeiro procura nos targets predefinidos
        foreach (Transform target in predefinedLookTargets)
        {
            if (target != null && target.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
            {
                return target;
            }
        }
        
        // Se não encontrar, procura na cena
        GameObject found = GameObject.Find(targetName);
        return found != null ? found.transform : null;
    }
    
    // Getters
    public bool IsAnimating => isAnimating;
    public bool PlayerControlsDisabled => playerControlsDisabled;
}
