using UnityEngine;
using Yarn.Unity;

public class InteractableStoryObject : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string storyNodeName;
    [SerializeField] private DialogueRunner dialogueRunner;
    
    [Header("Cutscene Effects")]
    [SerializeField] private bool hasCutsceneEffects = false;
    [SerializeField] private Light[] lightsToAnimate;
    [SerializeField] private float lightAnimationDuration = 2f;
    [SerializeField] private AnimationCurve lightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Memory Collection")]
    [SerializeField] private bool isMemoryObject = false;
    [SerializeField] private string memoryName;
    
    private bool canInteract;
    private bool hasBeenInteracted = false;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CanCurrentlyInteract())
        {
            canInteract = true;
            // Mostrar prompt de interação
            ShowInteractionPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            // Esconder prompt de interação
            HideInteractionPrompt();
        }
    }

    private void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E) && !dialogueRunner.IsDialogueRunning)
        {
            Interact();
        }
    }

    private void Interact()
    {
        // Play interaction sound
        if (interactionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }

        // Start cutscene effects if enabled
        if (hasCutsceneEffects)
        {
            StartCutsceneEffects();
        }

        // Start dialogue
        if (!string.IsNullOrEmpty(storyNodeName))
        {
            dialogueRunner.StartDialogue(storyNodeName);
        }

        // Handle memory collection
        if (isMemoryObject && !hasBeenInteracted)
        {
            CollectMemory();
        }

        hasBeenInteracted = true;
        HideInteractionPrompt();
    }

    private void StartCutsceneEffects()
    {
        // Animate lights
        if (lightsToAnimate != null && lightsToAnimate.Length > 0)
        {
            StartCoroutine(AnimateLights());
        }
    }

    private System.Collections.IEnumerator AnimateLights()
    {
        float[] originalIntensities = new float[lightsToAnimate.Length];
        
        // Store original intensities
        for (int i = 0; i < lightsToAnimate.Length; i++)
        {
            if (lightsToAnimate[i] != null)
                originalIntensities[i] = lightsToAnimate[i].intensity;
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < lightAnimationDuration)
        {
            float normalizedTime = elapsedTime / lightAnimationDuration;
            float curveValue = lightCurve.Evaluate(normalizedTime);

            for (int i = 0; i < lightsToAnimate.Length; i++)
            {
                if (lightsToAnimate[i] != null)
                {
                    lightsToAnimate[i].intensity = originalIntensities[i] * (1f + curveValue);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset to original intensities
        for (int i = 0; i < lightsToAnimate.Length; i++)
        {
            if (lightsToAnimate[i] != null)
                lightsToAnimate[i].intensity = originalIntensities[i];
        }
    }

    private void CollectMemory()
    {
        if (GameManager.Instance != null && !string.IsNullOrEmpty(memoryName))
        {
            GameManager.Instance.OnObjectFound(memoryName);
        }
    }

    private bool CanCurrentlyInteract()
    {
        // Check if player can interact based on game state
        return GameManager.Instance == null || GameManager.Instance.CanInteract();
    }

    private void ShowInteractionPrompt()
    {
        // TODO: Implementar UI prompt
        Debug.Log("Press E to interact");
    }

    private void HideInteractionPrompt()
    {
        // TODO: Esconder UI prompt
    }
}