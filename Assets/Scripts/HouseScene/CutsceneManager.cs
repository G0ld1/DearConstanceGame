using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System;
using Yarn.Unity;

public class CutsceneManager : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private PlayerMovementScript playerMovement;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraRoot;
    
    [Header("Camera Management")]
    [SerializeField] private CutsceneCameraManager cameraManager;
    
    [Header("Yarn Spinner")]
    [SerializeField] private DialogueRunner dialogueRunner;
   
    
    [Header("Cutscene Sequences")]
    [SerializeField] private List<CutsceneSequence> cutsceneSequences = new List<CutsceneSequence>();
    
    private bool isCutscenePlaying = false;
    private CutsceneSequence currentSequence;
    private int currentStepIndex = 0;
    
    // Estados salvos da câmera
    private CameraState savedCameraState;
    
    // Controle de diálogo
    private bool isDialogueActive = false;
    private Action onDialogueComplete;
    
    // Eventos
    public static event Action<string> OnCutsceneStarted;
    public static event Action<string> OnCutsceneEnded;
    public static event Action<string> OnInteractionRequired;
    
    private void Start()
    {
        // Setup timeline events
        timeline.played += OnTimelineStart;
        timeline.stopped += OnTimelineEnd;
        
        // Subscribe to interaction events
        InteractableObject.OnObjectInteracted += HandleObjectInteraction;
        
        // Setup Yarn Spinner events
        SetupYarnEvents();
    }
    
    private void SetupYarnEvents()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart.AddListener(OnYarnDialogueStart);
            dialogueRunner.onDialogueComplete.AddListener(OnYarnDialogueComplete);
        }
    }
    
    private void OnDestroy()
    {
        InteractableObject.OnObjectInteracted -= HandleObjectInteraction;
        
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart.RemoveListener(OnYarnDialogueStart);
            dialogueRunner.onDialogueComplete.RemoveListener(OnYarnDialogueComplete);
        }
    }

    public void StartCutsceneSequence(string sequenceId)
    {
        var sequence = cutsceneSequences.Find(s => s.sequenceId == sequenceId);
        if (sequence != null)
        {
            currentSequence = sequence;
            currentStepIndex = 0;
            ExecuteCurrentStep();
        }
        else
        {
            Debug.LogError($"Cutscene sequence '{sequenceId}' not found!");
        }
    }

    private void ExecuteCurrentStep()
    {
        if (currentSequence == null || currentStepIndex >= currentSequence.steps.Count)
        {
            FinishSequence();
            return;
        }

        var currentStep = currentSequence.steps[currentStepIndex];
        
        switch (currentStep.stepType)
        {
            case CutsceneStepType.Timeline:
                PlayTimelineStep(currentStep);
                break;
                
            case CutsceneStepType.Interaction:
                SetupInteractionStep(currentStep);
                break;
                
            case CutsceneStepType.YarnDialogue:
                PlayYarnDialogueStep(currentStep);
                break;
                
            case CutsceneStepType.Wait:
                PlayWaitStep(currentStep);
                break;
        }
    }

    private void PlayTimelineStep(CutsceneStep step)
    {
        if (step.timelineAsset != null)
        {
            SaveCameraState();
            SetPlayerControl(false);
            
            // Ativa câmera específica para este step se definida
            if (!string.IsNullOrEmpty(step.cutsceneCameraId) && cameraManager != null)
            {
                cameraManager.ActivateCutsceneCamera(step.cutsceneCameraId, false);
            }
            
            timeline.playableAsset = step.timelineAsset;
            SetupTimelineBindings();
            timeline.Play();
            
            OnCutsceneStarted?.Invoke(step.stepId);
        }
        else
        {
            Debug.LogError($"Timeline asset is null for step {step.stepId}");
            NextStep();
        }
    }

    private void SetupInteractionStep(CutsceneStep step)
    {
        // Ativa câmera específica para interação se definida
        if (!string.IsNullOrEmpty(step.cutsceneCameraId) && cameraManager != null)
        {
            cameraManager.ActivateCutsceneCamera(step.cutsceneCameraId, step.allowCameraInteraction);
        }
        
        // Ativa objetos interativos específicos
        foreach (var interactableId in step.interactableIds)
        {
            var interactable = FindInteractableById(interactableId);
            if (interactable != null)
            {
                interactable.EnableInteraction();
            }
        }
        
        // Define controle do player baseado no step
        SetPlayerControl(step.allowPlayerMovement, step.allowCameraMovement);
        
        OnInteractionRequired?.Invoke(step.stepId);
        Debug.Log($"Waiting for interaction: {step.description}");
    }

    private void PlayYarnDialogueStep(CutsceneStep step)
    {
        if (!string.IsNullOrEmpty(step.yarnNodeName) && dialogueRunner != null)
        {
            isDialogueActive = true;
            onDialogueComplete = () => NextStep();
            
            
            dialogueRunner.StartDialogue(step.yarnNodeName);
        }
        else
        {
            Debug.LogError($"Yarn node name is empty or DialogueRunner is null for step {step.stepId}");
            NextStep();
        }
    }

    private void PlayWaitStep(CutsceneStep step)
    {
        StartCoroutine(WaitCoroutine(step.waitDuration));
    }

    private System.Collections.IEnumerator WaitCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        NextStep();
    }

    private void OnYarnDialogueStart()
    {
        isDialogueActive = true;
        Debug.Log("Yarn dialogue started");
    }

    private void OnYarnDialogueComplete()
    {
        isDialogueActive = false;
        Debug.Log("Yarn dialogue completed");
        
        // Chama o callback se houver
        onDialogueComplete?.Invoke();
        onDialogueComplete = null;
    }

    private void HandleObjectInteraction(string objectId)
    {
        if (currentSequence == null) return;
        
        var currentStep = currentSequence.steps[currentStepIndex];
        
        if (currentStep.stepType == CutsceneStepType.Interaction && 
            currentStep.interactableIds.Contains(objectId))
        {
            // Desativa todos os interactables do step atual
            foreach (var interactableId in currentStep.interactableIds)
            {
                var interactable = FindInteractableById(interactableId);
                if (interactable != null)
                {
                    interactable.DisableInteraction();
                }
            }
            
            NextStep();
        }
    }

    private void NextStep()
    {
        currentStepIndex++;
        ExecuteCurrentStep();
    }

    private void FinishSequence()
    {
        SetPlayerControl(true);
        
        // Volta para câmera do player
        if (cameraManager != null)
        {
            cameraManager.DeactivateCurrentCutsceneCamera();
        }
        
        //RestoreCameraState();
        
        OnCutsceneEnded?.Invoke(currentSequence.sequenceId);
        Debug.Log($"Cutscene sequence '{currentSequence.sequenceId}' completed!");
        
        currentSequence = null;
    }

    private void OnTimelineStart(PlayableDirector director)
    {
        isCutscenePlaying = true;
    }

    private void OnTimelineEnd(PlayableDirector director)
    {
        isCutscenePlaying = false;
        NextStep();
    }

    private void SetupTimelineBindings()
    {
        foreach (var output in timeline.playableAsset.outputs)
        {
            if (output.streamName == "Camera Track")
            {
                timeline.SetGenericBinding(output.sourceObject, playerCamera);
            }
            else if (output.streamName == "Camera Root Track")
            {
                timeline.SetGenericBinding(output.sourceObject, cameraRoot);
            }
        }
    }

    private void SetPlayerControl(bool allowMovement, bool allowCamera = true)
    {
        if (playerMovement != null)
        {
            playerMovement.SetPlayerControl(allowMovement, allowCamera);
        }
    }

    private void SaveCameraState()
    {
        savedCameraState = new CameraState
        {
            cameraPosition = playerCamera.transform.localPosition,
            cameraRotation = playerCamera.transform.localRotation,
            cameraRootPosition = cameraRoot.transform.localPosition,
            cameraRootRotation = cameraRoot.transform.localRotation
        };
    }

    private void RestoreCameraState()
    {
        if (savedCameraState != null)
        {
            playerCamera.transform.localPosition = savedCameraState.cameraPosition;
            playerCamera.transform.localRotation = savedCameraState.cameraRotation;
            cameraRoot.transform.localPosition = savedCameraState.cameraRootPosition;
            cameraRoot.transform.localRotation = savedCameraState.cameraRootRotation;
            
            // IMPORTANTE: Reset das variáveis do PlayerMovementScript
            if (playerMovement != null)
            {
                playerMovement.ResetCameraRotation();
            }
        }
    }

    private InteractableObject FindInteractableById(string id)
    {
        var allInteractables = FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        return System.Array.Find(allInteractables, obj => obj.ObjectId == id);
    }

    // Yarn Commands - para chamar do Yarn Spinner
    [YarnCommand("start_cutscene")]
    public void StartCutsceneFromYarn(string sequenceId)
    {
        StartCutsceneSequence(sequenceId);
    }

    [YarnCommand("play_timeline")]
    public void PlayTimelineFromYarn(string timelineName)
    {
        
    }

    public bool IsCutscenePlaying => isCutscenePlaying || currentSequence != null || isDialogueActive;
}
