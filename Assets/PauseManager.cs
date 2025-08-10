using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private SoundManager soundManager;
    
    private bool isGamePaused = false;
    private List<AudioSource> pausedAudioSources = new List<AudioSource>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PauseGame()
    {
        if (isGamePaused) return;
        
        isGamePaused = true;
        
        // Don't stop dialogue - just pause all audio sources that are playing
        PauseAllDialogueAudio();
        
        Debug.Log("Game paused - audio paused but dialogue continues");
    }
    
    public void UnpauseGame()
    {
        if (!isGamePaused) return;
        
        isGamePaused = false;
        
        // Resume all paused audio sources
        ResumeAllDialogueAudio();
        
        Debug.Log("Game unpaused - audio resumed");
    }
    
    private void PauseAllDialogueAudio()
    {
        pausedAudioSources.Clear();
        
        // Find all AudioSources that are currently playing
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        Debug.Log($"Found {allAudioSources.Length} total audio sources");
        
        foreach (AudioSource source in allAudioSources)
        {
            // Only pause dialogue-related audio (you can customize this logic)
            if (source.isPlaying && IsDialogueAudio(source))
            {
                Debug.Log($"Pausing audio source: {source.gameObject.name}");
                source.Pause();
                pausedAudioSources.Add(source);
            }
        }
        
        Debug.Log($"Paused {pausedAudioSources.Count} dialogue audio sources");
        
        // Also pause through SoundManager if it has specific dialogue audio methods
        if (soundManager != null)
        {
            soundManager.PauseDialogueAudio();
        }
    }
    
    private void ResumeAllDialogueAudio()
    {
        Debug.Log($"Attempting to resume {pausedAudioSources.Count} audio sources");
        
        // Resume all audio sources that were paused
        for (int i = 0; i < pausedAudioSources.Count; i++)
        {
            AudioSource source = pausedAudioSources[i];
            if (source != null)
            {
                Debug.Log($"Resuming audio source: {source.gameObject.name}");
                
                // Try multiple approaches to resume
                source.UnPause();
                
                // If UnPause doesn't work, try Play() if the clip is still there
                if (!source.isPlaying && source.clip != null)
                {
                    Debug.Log($"UnPause failed for {source.gameObject.name}, trying Play()");
                    source.Play();
                }
            }
            else
            {
                Debug.LogWarning($"Audio source at index {i} is null - may have been destroyed");
            }
        }
        
        pausedAudioSources.Clear();
        
        // Also resume through SoundManager
        if (soundManager != null)
        {
            soundManager.ResumeDialogueAudio();
        }
    }
    
    private bool IsDialogueAudio(AudioSource source)
    {
        // Customize this logic based on how you identify dialogue audio
        // Option 1: By tag
        if (source.gameObject.CompareTag("VoiceLine") || source.gameObject.CompareTag("Dialogue"))
        {
            Debug.Log($"Found dialogue audio by tag: {source.gameObject.name}");
            return true;
        }
        
      
        
        return false;
    }
    
    // Method to manually check what audio sources are currently playing (for debugging)
    [ContextMenu("Debug Audio Sources")]
    public void DebugAudioSources()
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        Debug.Log($"=== AUDIO SOURCES DEBUG ({allAudioSources.Length} total) ===");
        
        foreach (AudioSource source in allAudioSources)
        {
            if (source.isPlaying)
            {
                string clipName = source.clip != null ? source.clip.name : "No clip";
                Debug.Log($"PLAYING: {source.gameObject.name} - Clip: {clipName} - Tag: {source.gameObject.tag}");
            }
        }
    }
    
    public bool IsGamePaused => isGamePaused;
}
