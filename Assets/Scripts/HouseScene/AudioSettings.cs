using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 1f;
    [Range(0f, 1f)] public float uiVolume = 1f;
    
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string VOICE_VOLUME_KEY = "VoiceVolume";
    private const string AMBIENT_VOLUME_KEY = "AmbientVolume";
    private const string UI_VOLUME_KEY = "UIVolume";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAudioSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        ApplyAudioSettings();
    }
    
    public void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        voiceVolume = PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 1f);
        ambientVolume = PlayerPrefs.GetFloat(AMBIENT_VOLUME_KEY, 1f);
        uiVolume = PlayerPrefs.GetFloat(UI_VOLUME_KEY, 1f);
    }
    
    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, voiceVolume);
        PlayerPrefs.SetFloat(AMBIENT_VOLUME_KEY, ambientVolume);
        PlayerPrefs.SetFloat(UI_VOLUME_KEY, uiVolume);
        PlayerPrefs.Save();
    }
    
    public void ApplyAudioSettings()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(sfxVolume * masterVolume);
            SoundManager.Instance.SetAmbientVolume(ambientVolume * masterVolume);
            SoundManager.Instance.SetUIVolume(uiVolume * masterVolume);
        }
        
        // Apply voice volume to VoiceLineDialoguePresenter
        var voicePresenter = FindFirstObjectByType<VoiceLineDialoguePresenter>();
        if (voicePresenter != null)
        {
            voicePresenter.SetVoiceVolume(voiceVolume * masterVolume);
        }
    }
    
    // Methods to be called by UI sliders
    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = volume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = volume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }
    
    public void SetUIVolume(float volume)
    {
        uiVolume = volume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }
}
