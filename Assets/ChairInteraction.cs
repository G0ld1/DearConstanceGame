using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class ChairInteraction : MonoBehaviour
{
    public static ChairInteraction Instance { get; private set; }
    
    [Header("Chair Settings")]
    [SerializeField] private Transform sittingPosition;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private GameObject interactionPrompt;
    
    [Header("Chair Rotation")]
    [SerializeField] private float rotationDegrees = 90f; // Graus para rodar
    [SerializeField] private float rotationDuration = 1.0f; // Duração da rotação em segundos
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioClip sittingAudioClip;
    [SerializeField] private AudioSource audioSource;
    
    [Header("References")]
    [SerializeField] private PlayerMovementScript playerMovement;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CameraWallAvoidance annoyingscript;
    
    
    private bool playerInRange = false;
    private bool playerSitting = false;
    private bool audioPlaying = false;
    private bool chairRotating = false;
    
    private Vector3 originalPlayerPosition;
    private Quaternion originalPlayerRotation;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform originalCameraParent;
    
    // Para guardar rotação original da cadeira
    private Quaternion originalChairRotation;
    
    public System.Action OnAudioCompleted;
    
    void Start()
    {
        Instance = this;
        
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovementScript>();
            
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        // Guarda a rotação original da cadeira
        originalChairRotation = transform.rotation;
    }
    
    void Update()
    {
        if (playerInRange && !playerSitting && !audioPlaying && !chairRotating && Input.GetKeyDown(KeyCode.E))
        {
            SitDown();
        }
        else if (playerSitting && !audioPlaying && !chairRotating && Input.GetKeyDown(KeyCode.E))
        {
            StandUp();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
                
            if (playerSitting && !audioPlaying && !chairRotating)
                StandUp();
        }
    }

    public void SitDown()
    {
        if (playerSitting || audioPlaying || chairRotating) return;

        // Guarda posições originais
        originalPlayerPosition = playerMovement.transform.position;
        originalPlayerRotation = playerMovement.transform.rotation;
        originalCameraPosition = playerCamera.transform.localPosition;
        originalCameraRotation = playerCamera.transform.localRotation;
        originalCameraParent = playerCamera.transform.parent;

        annoyingscript.enabled = false;

        // Bloqueia movimento do jogador
        playerMovement.BlockMovement();

        CharacterController controller = playerMovement.GetComponent<CharacterController>();
        controller.enabled = false;

        // Move o jogador
        playerMovement.transform.position = sittingPosition.position;
        playerMovement.transform.rotation = sittingPosition.rotation;

        controller.enabled = true;
        playerSitting = true;

        // Atualiza UI
        if (interactionPrompt != null)
        {
            interactionPrompt.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rotating chair...";
        }

        // Inicia a rotação da cadeira
        StartCoroutine(RotateChairSequence());
        
        GameManager.Instance.OnReachedStudy();
    }
    
    private IEnumerator RotateChairSequence()
    {
        chairRotating = true;
        
        // Calcula a rotação alvo baseada nos graus especificados
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, rotationDegrees, 0);
        
        float elapsed = 0f;
        
        // Anima a rotação da cadeira
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / rotationDuration;
            float curveValue = rotationCurve.Evaluate(progress);
            
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, curveValue);
            
            // Se o jogador estiver sentado, roda ele junto com a cadeira
            if (playerSitting)
            {
                playerMovement.transform.rotation = Quaternion.Lerp(
                    originalPlayerRotation, 
                    originalPlayerRotation * Quaternion.Euler(0, rotationDegrees, 0), 
                    curveValue
                );
            }
            
            yield return null;
        }
        
        // Garante que chegou à rotação final
        transform.rotation = targetRotation;
        if (playerSitting)
        {
            playerMovement.transform.rotation = originalPlayerRotation * Quaternion.Euler(0, rotationDegrees, 0);
        }
        
        chairRotating = false;
        
        // Atualiza UI
        if (interactionPrompt != null)
        {
            interactionPrompt.GetComponentInChildren<UnityEngine.UI.Text>().text = "Playing audio...";
        }
                
        // Agora inicia o áudio
        //StartCoroutine(PlayAudioAndWait());
    }
    
    private IEnumerator PlayAudioAndWait()
    {
        audioPlaying = true;
        
        if (audioSource != null && sittingAudioClip != null)
        {
            audioSource.PlayOneShot(sittingAudioClip);
            Debug.Log("Audio started playing");
            yield return new WaitForSeconds(sittingAudioClip.length);
        }
        else
        {
            Debug.LogWarning("No audio clip or audio source configured");
        }
        
        audioPlaying = false;
        Debug.Log("Audio finished - ready for next actions");

    }

    public void StandUp()
    {
        if (!playerSitting || audioPlaying || chairRotating) return;

        StartCoroutine(StandUpSequence());
    }
    
    private IEnumerator StandUpSequence()
    {
        chairRotating = true;
        
        // Roda a cadeira de volta à posição original
        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;
        
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / rotationDuration;
            float curveValue = rotationCurve.Evaluate(progress);
            
            transform.rotation = Quaternion.Lerp(startRotation, originalChairRotation, curveValue);
            yield return null;
        }
        
        transform.rotation = originalChairRotation;
        chairRotating = false;
        
        // Depois move o jogador de volta
        playerMovement.UnblockMovement();
        
        CharacterController controller = playerMovement.GetComponent<CharacterController>();
        controller.enabled = false;
        
        playerMovement.transform.position = originalPlayerPosition;
        playerMovement.transform.rotation = originalPlayerRotation;
        
        controller.enabled = true;

        // Restaura câmera
        if (originalCameraParent != null)
        {
            playerCamera.transform.SetParent(originalCameraParent);
            playerCamera.transform.localPosition = originalCameraPosition;
            playerCamera.transform.localRotation = originalCameraRotation;
        }

        playerSitting = false;

        // Atualiza UI
        if (interactionPrompt != null && playerInRange)
        {
            interactionPrompt.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press E to sit";
        }

        Debug.Log("Player stood up");
        annoyingscript.enabled = true;
    }
    
    [YarnCommand("stand_from_chair")]
    public static void StandFromYarn()
    {
        if (Instance != null && Instance.playerSitting && !Instance.audioPlaying && !Instance.chairRotating)
        {
            Instance.StandUp();
        }
    }
    
    // Getters
    public bool IsPlayerSitting => playerSitting;
    public bool IsPlayerInRange => playerInRange;
    public bool IsAudioPlaying => audioPlaying;
    public bool IsChairRotating => chairRotating;
}
