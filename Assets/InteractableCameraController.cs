using UnityEngine;
using Unity.Cinemachine;

public class InteractableCameraController : MonoBehaviour
{
    [Header("Camera Control")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float verticalLookLimit = 60f;
    
    [Header("Input")]
    [SerializeField] private bool invertY = false;
    
    private bool isActive = false;
    private float currentXRotation = 0f;
    private float currentYRotation = 0f;
    
    private void Start()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineCamera>();
        }
        
        // Inicializa rotações baseadas na rotação atual da câmera
        Vector3 currentRotation = transform.eulerAngles;
        currentYRotation = currentRotation.y;
        currentXRotation = currentRotation.x;
        
        // Normaliza X rotation
        if (currentXRotation > 180f)
        {
            currentXRotation -= 360f;
        }
        
        // Inicia desativado
        SetActive(false);
    }
    
    private void Update()
    {
        if (isActive)
        {
            HandleCameraInput();
        }
    }
    
    private void HandleCameraInput()
    {
        // Pega input do mouse
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        
        if (invertY)
        {
            mouseY = -mouseY;
        }
        
        // Atualiza rotações
        currentYRotation += mouseX;
        currentXRotation -= mouseY;
        
        // Limita rotação vertical
        currentXRotation = Mathf.Clamp(currentXRotation, -verticalLookLimit, verticalLookLimit);
        
        // Aplica rotação
        transform.rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0f);
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (active)
        {
            // Atualiza rotações baseadas na posição atual quando ativa
            Vector3 currentRotation = transform.eulerAngles;
            currentYRotation = currentRotation.y;
            currentXRotation = currentRotation.x;
            
            if (currentXRotation > 180f)
            {
                currentXRotation -= 360f;
            }
        }
        
        Debug.Log($"InteractableCameraController {gameObject.name} set to: {active}");
    }
    
    public CinemachineCamera VirtualCamera => virtualCamera;
}
