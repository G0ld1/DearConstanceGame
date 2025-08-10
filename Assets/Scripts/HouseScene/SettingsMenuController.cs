using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Settings UI")]
    [SerializeField] private GameObject settingsMenuPanel;
    [SerializeField] private Button backButton;
    
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;
    
    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
  
    [Header("References")]
    [SerializeField] private PauseMenuAnimationController pauseMenuController;
    
    private bool isSettingsOpen = false;
    
    // Audio volume values
    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float voiceVolume = 1f;
    
    private void Start()
    {
        SetupSettingsMenu();
        
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }
    }
    
    private void SetupSettingsMenu()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseSettingsMenu);
        }
        
        // Setup audio sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
        }
        
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
        
        LoadSettings();
    }
    
    public void OpenSettingsMenu()
    {
        Debug.Log("Opening Settings Menu");
        
        isSettingsOpen = true;
        
        // Don't change timeScale here - let PauseMenuAnimationController handle it
        
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(true);
        }
        
        if (pauseMenuController != null)
        {
            pauseMenuController.SetButtonsInteractable(false);
        }
    }
    
    public void CloseSettingsMenu()
    {
        Debug.Log("Closing Settings Menu");
        
        isSettingsOpen = false;
        
        SaveSettings();
        
        // Don't change timeScale here - let PauseMenuAnimationController handle it
        
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }
        
        if (pauseMenuController != null)
        {
            pauseMenuController.SetButtonsInteractable(true);
        }
    }
    
    // === AUDIO SETTINGS HANDLERS ===
    
    private void OnMasterVolumeChanged(float value)
    {
        masterVolume = value;
        ApplyAudioSettings();
        Debug.Log($"Master Volume: {value}");
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        musicVolume = value;
        ApplyAudioSettings();
        Debug.Log($"Music Volume: {value}");
    }
    
    private void OnVoiceVolumeChanged(float value)
    {
        voiceVolume = value;
        ApplyAudioSettings();
        Debug.Log($"Voice Volume: {value}");
    }
    
    private void ApplyAudioSettings()
    {
        // Apply to SoundManager if it exists
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(masterVolume * musicVolume); // Using musicVolume for SFX
            SoundManager.Instance.SetAmbientVolume(masterVolume * musicVolume);
            SoundManager.Instance.SetUIVolume(masterVolume * musicVolume);
        }
        
        // Apply voice volume to VoiceLineDialoguePresenter
        var voicePresenter = FindFirstObjectByType<VoiceLineDialoguePresenter>();
        if (voicePresenter != null)
        {
            voicePresenter.SetVoiceVolume(masterVolume * voiceVolume);
        }
        
        // Set master volume for overall audio control
        AudioListener.volume = masterVolume;
    }
    
    private void OnResolutionChanged(int index)
    {
        Debug.Log($"Resolution changed to index: {index}");
    }
    
    // === SAVE/LOAD SETTINGS ===
    
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
        
        PlayerPrefs.Save();
        Debug.Log("Settings saved");
    }
    
    private void LoadSettings()
    {
        // Load saved values
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 0.8f);
        
        // Update sliders to reflect loaded values
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
        }
        
        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.value = voiceVolume;
        }
        
        // Apply the loaded settings
        ApplyAudioSettings();
        
        Debug.Log("Settings loaded");
    }
    
    // === PUBLIC PROPERTIES ===
    
    public bool IsSettingsOpen => isSettingsOpen;
    
    // Public methods for external access if needed
    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = volume;
        ApplyAudioSettings();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = volume;
        ApplyAudioSettings();
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = volume;
        if (voiceVolumeSlider != null)
            voiceVolumeSlider.value = volume;
        ApplyAudioSettings();
    }
}
