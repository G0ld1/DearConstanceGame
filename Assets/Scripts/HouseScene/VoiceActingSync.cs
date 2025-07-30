using UnityEngine;
using Yarn.Unity;

public class VoiceActingSync : MonoBehaviour
{
    [Header("Voice Settings")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private float silencePadding = 0.5f;
    
    // Static instance para aceder no método static
    private static VoiceActingSync instance;
    
    private DialogueRunner dialogueRunner;
    private LineAdvancer lineAdvancer; // Provavelmente o que controla o avanço

    private void Start()
    {
        // Set static instance
        instance = this;
        
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        lineAdvancer = FindFirstObjectByType<LineAdvancer>();
        
        if (voiceSource == null)
            voiceSource = gameObject.AddComponent<AudioSource>();
        
     
        
        Debug.Log($"VoiceActingSync initialized. LineView found: {lineAdvancer != null}");
    }

    [YarnCommand("playvoice")]
    public static void PlayVoiceCommand(string clipName)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.PlayVoiceClip(clipName));
        }
        else
        {
            Debug.LogError("VoiceActingSync instance not found!");
        }
    }

    private System.Collections.IEnumerator PlayVoiceClip(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>($"VoiceActing/{clipName}");
        
        if (clip != null)
        {
            Debug.Log($"Playing voice clip: {clipName}");
            
            // Disable line view interactions
            if (lineAdvancer != null)
            {
                lineAdvancer.enabled = false;
            }
            
            // Play audio
            voiceSource.clip = clip;
            voiceSource.Play();
            
            // Wait for completion
            yield return new WaitForSeconds(clip.length + silencePadding);
            
            // Re-enable line view
            if (lineAdvancer != null)
            {
                lineAdvancer.enabled = true;
            }
            
            Debug.Log($"Voice clip finished: {clipName}");
        }
        else
        {
            Debug.LogWarning($"Voice clip not found: VoiceActing/{clipName}");
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
