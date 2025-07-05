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

    private CharacterController controller;
    private Camera cam;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity;
    private float xRotation = 0f;

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
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();

    }


    void FixedUpdate()
    {
        Move();
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
}
