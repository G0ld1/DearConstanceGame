using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovementScript : MonoBehaviour
{
    [Header("MoveInputs")]
    public float moveSpeed = 5f;
    public float lookSens = 2f;

    [Header("Pause Menu")]
    [SerializeField] private PauseMenuAnimationController pauseMenuController;
    [SerializeField] private GameObject pauseMenuPanel; // opcional para verificação

    [Header("Footsteps Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private float stepInterval = 0.5f; // Tempo entre passos
    [SerializeField] private float minVelocityForFootsteps = 0.1f; // Velocidade mínima para tocar footsteps

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.3f; // Aumentei para 0.3f
    [SerializeField] private LayerMask groundLayerMask = 1; // Layer do chão

    private CharacterController controller;
    private Camera cam;
    private Vector2 moveInput;
    private Vector2 lookInput;
    
    private Vector3 velocity;

    public float gravity = -9.81f;

    private bool isGrounded;
    private float xRotation = 0f;
    private bool isPaused = false;
    
    // Movement control
    private bool movementBlocked = false; // Nova variável

    // Footstep variables
    private float stepTimer = 0f;
    private bool wasMovingLastFrame = false;
    private int currentFootstepIndex = 0; 

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // Adiciona o input de pausa
        inputActions.Player.Pause.performed += ctx => TogglePause();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        
        // Setup footstep audio source
        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponent<AudioSource>();
            if (footstepAudioSource == null)
            {
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        
        SetCursorState(false); // Começa com cursor locked
    }

    void Update()
    {
        // Só processa se não estiver pausado
        if (!isPaused)
        {
            Look(); // Câmera sempre funciona se não pausado
            
            // Movimento só se não estiver bloqueado
            if (!movementBlocked)
            {
                Move();
                HandleFootsteps();
            }
            else
            {
                // Ainda aplica gravidade mesmo com movimento bloqueado
                ApplyGravityOnly();
            }

            // Sempre verifica se está no chão
            isGrounded = IsGroundedCheck();
        }
    }

    void Move()
    {
        // Movimento horizontal
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Movimento vertical (gravidade)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        // Combina movimento horizontal e vertical
        Vector3 totalMovement = move * moveSpeed * Time.deltaTime + velocity * Time.deltaTime;
        controller.Move(totalMovement);
    }
    
    private void ApplyGravityOnly()
    {
        // Só aplica gravidade quando movimento está bloqueado
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        
        // Move apenas pela gravidade (sem movimento horizontal)
        controller.Move(velocity * Time.deltaTime);
    }
    
    void Look()
    {
        xRotation -= lookInput.y * lookSens;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * lookSens);
    }

    // === MÉTODOS PÚBLICOS PARA CONTROLE DE MOVIMENTO ===

    /// <summary>
    /// Bloqueia o movimento do jogador (andar) mas mantém a câmera livre
    /// </summary>
    public void BlockMovement()
    {
        movementBlocked = true;
        Debug.Log("Player movement blocked - camera still free");
    }

    /// <summary>
    /// Desbloqueia o movimento do jogador
    /// </summary>
    public void UnblockMovement()
    {
        movementBlocked = false;
        Debug.Log("Player movement unblocked");
    }

    /// <summary>
    /// Define o estado de bloqueio do movimento
    /// </summary>
    /// <param name="blocked">True para bloquear, false para desbloquear</param>
    public void SetMovementBlocked(bool blocked)
    {
        movementBlocked = blocked;
        Debug.Log($"Player movement {(blocked ? "blocked" : "unblocked")}");
    }

    // === GETTERS ===

    public bool IsMovementBlocked => movementBlocked;

    private void HandleFootsteps()
    {
        // Debug the movement values
        float moveInputMagnitude = moveInput.magnitude;
        float controllerVelocityMagnitude = controller.velocity.magnitude;
        
        // Check if player is moving
        bool isMoving = moveInputMagnitude > 0.1f && controllerVelocityMagnitude > minVelocityForFootsteps;
        
        // Only play footsteps if grounded and moving
        if (isGrounded && isMoving)
        {
            stepTimer += Time.deltaTime;
            
            // Adjust step interval based on movement speed (faster = more frequent steps)
            float currentStepInterval = stepInterval / (controllerVelocityMagnitude / moveSpeed);
            
            if (stepTimer >= currentStepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when not moving
            stepTimer = 0f;
        }
        
        wasMovingLastFrame = isMoving;
    }

    private void PlayFootstepSound()
    {
    

        // Use o clip atual em sequência em vez de aleatório
        AudioClip currentFootstep = footstepSounds[currentFootstepIndex];

        if (currentFootstep == null)
        {
            Debug.LogError("Selected footstep clip is null!");
            return;
        }


        footstepAudioSource.PlayOneShot(currentFootstep);

        // Avança para o próximo clip na sequência
        currentFootstepIndex = (currentFootstepIndex + 1) % footstepSounds.Length;
    }



    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            OpenPauseMenu();
        }
        else
        {
            ClosePauseMenu();
        }
    }

    private void OpenPauseMenu()
    {
        Debug.Log("Opening pause menu");
        
        // Mostra cursor
        SetCursorState(true);
        
        // Abre o menu através do controller
        if (pauseMenuController != null)
        {
            pauseMenuController.ShowPauseMenu();
        }
        else if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    private void ClosePauseMenu()
    {
        Debug.Log("Closing pause menu");
        
        // Esconde cursor
        SetCursorState(false);

        // Fecha o menu
        if (pauseMenuController != null)
        {
            pauseMenuController.HidePauseMenu();
        }
        else if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void SetCursorState(bool showCursor)
    {
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // Método público para outros scripts fecharem o menu
    public void ResumeGame()
    {
        isPaused = false;
        ClosePauseMenu();
    }

    // Propriedade para outros scripts verificarem se está pausado
    public bool IsPaused => isPaused;

    private bool IsGroundedCheck()
    {
        Vector3 rayStart = transform.position;
        Vector3 rayDirection = Vector3.down;
        float rayDistance = (controller.height / 2) + groundCheckDistance;
        
        // Teste SEM LayerMask primeiro
        bool raycastGrounded = Physics.Raycast(rayStart, rayDirection, rayDistance);
        
        return raycastGrounded;
    }


}
