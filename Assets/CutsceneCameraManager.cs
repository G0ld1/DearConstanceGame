using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class CutsceneCameraManager : MonoBehaviour
{
    [Header("Main Camera")]
    [SerializeField] private CinemachineCamera playerVirtualCamera;
    [SerializeField] private int defaultPlayerPriority = 10;
    
    [Header("Cutscene Cameras")]
    [SerializeField] private List<CutsceneCameraData> cutsceneCameras = new List<CutsceneCameraData>();
    
    private CinemachineCamera currentCutsceneCamera;
    private InteractableCameraController currentInteractableController;
    
    
    /// <summary>
    /// Ativa uma câmera de cutscene específica
    /// </summary>
    public void ActivateCutsceneCamera(string cameraId, bool allowInteraction = false)
    {
        var cameraData = cutsceneCameras.Find(c => c.cameraId == cameraId);
        
        if (cameraData != null && cameraData.virtualCamera != null)
        {
            // Desativa câmera atual se houver
            DeactivateCurrentCutsceneCamera();
            
            // Ativa nova câmera
            currentCutsceneCamera = cameraData.virtualCamera;
            currentCutsceneCamera.Priority = defaultPlayerPriority + 1;
            
            // Ativa controle interativo se necessário
            if (allowInteraction && cameraData.interactableController != null)
            {
                currentInteractableController = cameraData.interactableController;
                currentInteractableController.SetActive(true);
            }
            
            Debug.Log($"Activated cutscene camera: {cameraId}");
        }
        else
        {
            Debug.LogError($"Cutscene camera '{cameraId}' not found!");
        }
    }
    
    /// <summary>
    /// Desativa a câmera de cutscene atual e volta para a do player
    /// </summary>
    public void DeactivateCurrentCutsceneCamera()
    {
        if (currentCutsceneCamera != null)
        {
            currentCutsceneCamera.Priority = 0;
            currentCutsceneCamera = null;
        }
        
        if (currentInteractableController != null)
        {
            currentInteractableController.SetActive(false);
            currentInteractableController = null;
        }
        
        // Certifica que a câmera do player tem prioridade
        if (playerVirtualCamera != null)
        {
            playerVirtualCamera.Priority = defaultPlayerPriority;
        }
        
        Debug.Log("Deactivated cutscene camera, returned to player camera");
    }
    
    /// <summary>
    /// Ativa controle interativo na câmera atual
    /// </summary>
    public void EnableCameraInteraction()
    {
        if (currentInteractableController != null)
        {
            currentInteractableController.SetActive(true);
        }
    }
    
    /// <summary>
    /// Desativa controle interativo na câmera atual
    /// </summary>
    public void DisableCameraInteraction()
    {
        if (currentInteractableController != null)
        {
            currentInteractableController.SetActive(false);
        }
    }
    
    public bool IsUsingCutsceneCamera => currentCutsceneCamera != null;
}

[System.Serializable]
public class CutsceneCameraData
{
    public string cameraId;
    public CinemachineCamera virtualCamera;
    public InteractableCameraController interactableController; 
}
