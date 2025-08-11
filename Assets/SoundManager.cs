using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepsSource;
    [SerializeField] private AudioSource ConstanceSource;
    [SerializeField] private AudioSource dialogueAudioSource;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float mainVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float voiceVolume = 1f;
    
    // Base volumes for each source (set once and preserved)
    private float sfxBaseVolume = 1f;
    private float footstepsBaseVolume = 1f;
    private float constanceBaseVolume = 1f;
    private float dialogueBaseVolume = 1f;
    
    private List<AudioSource> pausedDialogueSources = new List<AudioSource>();
    
    private bool dialogueAudioWasPlaying = false;
    private bool voiceLineWasPlaying = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Store base volumes when the object is created
            StoreBaseVolumes();
            
            // Load saved volume settings
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Apply loaded volumes
        ApplyAllVolumes();
    }
    
    private void StoreBaseVolumes()
    {
        // Store the initial volumes set in the Inspector
        if (sfxSource != null) sfxBaseVolume = sfxSource.volume;
        if (footstepsSource != null) footstepsBaseVolume = footstepsSource.volume;
        if (ConstanceSource != null) constanceBaseVolume = ConstanceSource.volume;
        if (dialogueAudioSource != null) dialogueBaseVolume = dialogueAudioSource.volume;
    }
    
    // === VOLUME CONTROL METHODS ===
    
    public void SetMainVolume(float volume)
    {
        mainVolume = Mathf.Clamp01(volume);
        ApplyAllVolumes();
        SaveVolumeSettings();
        Debug.Log($"Main Volume set to: {mainVolume}");
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolumes();
        SaveVolumeSettings();
        Debug.Log($"Music Volume set to: {musicVolume}");
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        ApplyVoiceVolumes();
        SaveVolumeSettings();
        Debug.Log($"Voice Volume set to: {voiceVolume}");
    }
    
    // === VOLUME APPLICATION METHODS ===
    
    private void ApplyAllVolumes()
    {
        ApplyMusicVolumes();
        ApplyVoiceVolumes();
    }
    
    private void ApplyMusicVolumes()
    {
        // Music category: ConstanceSource, sfxSource, footstepsSource
        float finalMusicVolume = mainVolume * musicVolume;
        
        if (ConstanceSource != null)
            ConstanceSource.volume = constanceBaseVolume * mainVolume;
            
        if (sfxSource != null)
            sfxSource.volume = sfxBaseVolume * finalMusicVolume;
            
        if (footstepsSource != null)
            footstepsSource.volume = footstepsBaseVolume * mainVolume;
    }
    
    private void ApplyVoiceVolumes()
    {
        // Voice category: dialogueAudioSource
        float finalVoiceVolume = mainVolume * voiceVolume;
        
        if (dialogueAudioSource != null)
            dialogueAudioSource.volume = dialogueBaseVolume * finalVoiceVolume;
    }
    
    // === SAVE/LOAD SETTINGS ===
    
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", mainVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
        PlayerPrefs.Save();
    }
    
    private void LoadVolumeSettings()
    {
        mainVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 0.8f);
    }
    
    // === GETTERS ===
    
    public float GetMainVolume() => mainVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetVoiceVolume() => voiceVolume;
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            // Volume já está aplicado pelo sistema de grupos
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    
    // === LEGACY VOLUME METHODS (For backward compatibility) ===
    
    [System.Obsolete("Use SetMusicVolume() instead")]
    public void SetSFXVolume(float volume)
    {
        SetMusicVolume(volume);
    }
    
    [System.Obsolete("Use SetMusicVolume() instead")]
    public void SetFootstepsVolume(float volume)
    {
        SetMusicVolume(volume);
    }
    
    [System.Obsolete("Use SetMusicVolume() instead")]
    public void SetConstanceVolume(float volume)
    {
        SetMusicVolume(volume);
    }
    
    [System.Obsolete("Use SetMusicVolume() instead")]
    public void SetAmbientVolume(float volume)
    {
        SetMusicVolume(volume);
    }
    
    [System.Obsolete("Use SetMusicVolume() instead")]
    public void SetUIVolume(float volume)
    {
        SetMusicVolume(volume);
    }
    
    // === DIALOGUE PAUSE/RESUME (unchanged) ===
    
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
}
