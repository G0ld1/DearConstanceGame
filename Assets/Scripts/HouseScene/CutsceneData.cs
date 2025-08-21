using UnityEngine;
using UnityEngine.Timeline;
using System.Collections.Generic;

[System.Serializable]
public class CutsceneSequence
{
    public string sequenceId;
    public string description;
    public List<CutsceneStep> steps = new List<CutsceneStep>();
}

[System.Serializable]
public class CutsceneStep
{
    public string stepId;
    public string description;
    public CutsceneStepType stepType;
    
    [Header("Timeline Step")]
    public TimelineAsset timelineAsset;
    public string cutsceneCameraId;
    
    [Header("Interaction Step")]
    public List<string> interactableIds = new List<string>();
    public bool allowPlayerMovement = false;
    public bool allowCameraMovement = true;
    public bool allowCameraInteraction = false;
    
    [Header("Yarn Dialogue Step")]
    public string yarnNodeName; // Nome do nó no Yarn Spinner
    
    [Header("Wait Step")]
    public float waitDuration = 1f;
}

public enum CutsceneStepType
{
    Timeline,
    Interaction,
    YarnDialogue,
    Wait
}

[System.Serializable]
public class CameraState
{
    public Vector3 cameraPosition;
    public Quaternion cameraRotation;
    public Vector3 cameraRootPosition;
    public Quaternion cameraRootRotation;
}
