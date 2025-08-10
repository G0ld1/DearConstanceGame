using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class VoiceLineManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string voiceLinesPath = "VoiceLines/"; // Resources folder path
    
    private DialogueRunner dialogueRunner;
    
    void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        
        // Subscribe to the correct line events
       // dialogueRunner.onLineUpdate.AddListener(OnLineUpdate);
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
    }
    

    
    void PlayVoiceLine(string lineTag)
    {
        // Only play if not already playing this voice line
        if (audioSource.clip == null || audioSource.clip.name != lineTag)
        {
            AudioClip voiceClip = Resources.Load<AudioClip>(voiceLinesPath + lineTag);
            
            if (voiceClip != null)
            {
                audioSource.clip = voiceClip;
                audioSource.Play();
                Debug.Log($"Playing voice line: {lineTag}");
            }
            else
            {
                Debug.LogWarning($"Voice line not found: {lineTag}");
            }
        }
    }
    
    void OnDialogueComplete()
    {
        // Stop audio when dialogue is complete
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (dialogueRunner != null)
        {
        //    dialogueRunner.onLineUpdate.RemoveListener(OnLineUpdate);
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        }
    }
}
