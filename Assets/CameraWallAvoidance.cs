using UnityEngine;

public class CameraWallAvoidance : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraTarget; // Posição ideal da câmera (nos olhos do jogador)
    
    [Header("Wall Avoidance Settings")]
    [SerializeField] private float maxDistance = 0.5f; // Distância máxima dos olhos até a câmera
    [SerializeField] private float minDistance = 0.1f; // Distância mínima da parede
    [SerializeField] private LayerMask wallLayerMask = -1; // Layers que são consideradas paredes
    [SerializeField] private float sphereRadius = 0.1f; // Raio da "cabeça" para detecção
    [SerializeField] private float smoothSpeed = 10f; // Velocidade de transição
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;
    
    private Vector3 targetPosition;
    private Vector3 currentPosition;
    
    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (cameraTarget == null)
            cameraTarget = transform; // Usa este próprio transform se não especificado
            
        // Posição inicial
        targetPosition = cameraTarget.position;
        currentPosition = targetPosition;
    }
    
    void LateUpdate()
    {
        UpdateCameraPosition();
    }
    
    private void UpdateCameraPosition()
    {
        // Posição ideal da câmera (nos olhos)
        Vector3 idealPosition = cameraTarget.position;
        
        // Direção da câmera (para frente)
        Vector3 cameraForward = playerCamera.transform.forward;
        
        // Posição da câmera um pouco atrás dos olhos
        Vector3 desiredCameraPosition = idealPosition - (cameraForward * maxDistance);
        
        // Raycast do olhos até a posição desejada da câmera
        Vector3 rayDirection = desiredCameraPosition - idealPosition;
        float rayDistance = rayDirection.magnitude;
        
        RaycastHit hit;
        bool hitWall = Physics.SphereCast(
            idealPosition, 
            sphereRadius, 
            rayDirection.normalized, 
            out hit, 
            rayDistance, 
            wallLayerMask
        );
        
        if (hitWall)
        {
            // Se atingiu uma parede, posiciona a câmera antes da parede
            float distanceToWall = hit.distance;
            float safeDistance = Mathf.Max(distanceToWall - minDistance, 0.01f);
            targetPosition = idealPosition + rayDirection.normalized * safeDistance;
        }
        else
        {
            // Se não atingiu parede, usa a posição desejada
            targetPosition = desiredCameraPosition;
        }
        
        // Suaviza o movimento da câmera
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * smoothSpeed);
        playerCamera.transform.position = currentPosition;
        
        // Debug
        if (showDebugRays)
        {
            Debug.DrawRay(idealPosition, rayDirection.normalized * rayDistance, hitWall ? Color.red : Color.green);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (cameraTarget == null) return;
        
        // Mostra a posição ideal
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(cameraTarget.position, 0.05f);
        
        // Mostra a área de detecção
        Gizmos.color = Color.yellow;
        Vector3 cameraForward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        Vector3 desiredPos = cameraTarget.position - (cameraForward * maxDistance);
        Gizmos.DrawWireSphere(desiredPos, sphereRadius);
        
        // Linha entre posição ideal e câmera
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(cameraTarget.position, desiredPos);
    }
}
