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

    private CharacterController controller;
    private Camera cam;
    private Vector2 moveInput;
    private Vector2 lookInput;
    
    private Vector3 velocity;

    public float gravity = -9.81f;

    private bool isGrounded;
    private float xRotation = 0f;
    private bool isPaused = false;

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
        SetCursorState(false); // Começa com cursor locked
    }

    void Update()
    {
        // Só processa movimento se não estiver pausado
        if (!isPaused)
        {
            Look();
            Move();

            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }

    void Move()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.fixedDeltaTime);
    }
    
    void Look()
    {
        xRotation -= lookInput.y * lookSens;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * lookSens);
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
        if (pauseMenuPanel != null)
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
}
