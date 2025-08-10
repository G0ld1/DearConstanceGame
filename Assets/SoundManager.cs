using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource uiSource;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;
    [SerializeField] private AudioClip objectInteractSound;
    [SerializeField] private AudioClip paperWriteSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Settings")]
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private bool isWalking = false;
    
    [Header("Dialogue Audio")]
    [SerializeField] private AudioSource dialogueAudioSource;
    
    
    private List<AudioSource> pausedDialogueSources = new List<AudioSource>();
    
    private Coroutine footstepCoroutine;
    private bool dialogueAudioWasPlaying = false;
    private bool voiceLineWasPlaying = false;
    
    void Awake()
    {
        // Singleton pattern
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
    
    // General SFX methods
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    
    public void PlayUISFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && uiSource != null)
        {
            uiSource.PlayOneShot(clip, volume);
        }
    }
    
    // Specific sound methods
    public void PlayDoorOpen()
    {
        PlaySFX(doorOpenSound);
    }
    
    public void PlayDoorClose()
    {
        PlaySFX(doorCloseSound);
    }
    
    public void PlayObjectInteract()
    {
        PlaySFX(objectInteractSound);
    }
    
    public void PlayPaperWrite()
    {
        PlaySFX(paperWriteSound);
    }
    
    public void PlayButtonClick()
    {
        PlayUISFX(buttonClickSound);
    }
    
    // Footstep system
    public void StartFootsteps()
    {
        if (!isWalking && footstepSounds.Length > 0)
        {
            isWalking = true;
            footstepCoroutine = StartCoroutine(FootstepLoop());
        }
    }
    
    public void StopFootsteps()
    {
        if (isWalking)
        {
            isWalking = false;
            if (footstepCoroutine != null)
            {
                StopCoroutine(footstepCoroutine);
                footstepCoroutine = null;
            }
        }
    }
    
    private IEnumerator FootstepLoop()
    {
        while (isWalking)
        {
            // Play random footstep sound
            if (footstepSounds.Length > 0)
            {
                AudioClip footstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
                PlaySFX(footstep, 0.7f);
            }
            
            yield return new WaitForSeconds(footstepInterval);
        }
    }
    
    // Volume controls
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
    }
    
    public void SetAmbientVolume(float volume)
    {
        if (ambientSource != null) ambientSource.volume = volume;
    }
    
    public void SetUIVolume(float volume)
    {
        if (uiSource != null) uiSource.volume = volume;
    }
    
    public void PauseDialogueAudio()
    {
        pausedDialogueSources.Clear();
        
        // Pause main dialogue audio
        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
        {
            dialogueAudioSource.Pause();
            pausedDialogueSources.Add(dialogueAudioSource);
        }
        
    
    }
    
    public void ResumeDialogueAudio()
    {
        // Resume all paused dialogue audio sources
        foreach (AudioSource source in pausedDialogueSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
        
        pausedDialogueSources.Clear();
    }
    
    // Optional: Method to check if any dialogue audio is playing
    public bool IsDialogueAudioPlaying()
    {
        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
            return true;
            
      
            
        return false;
    }
}
