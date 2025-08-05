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
            voiceVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
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
        

        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }
        
       
        if (pauseMenuController != null)
        {
            pauseMenuController.SetButtonsInteractable(true);
        }
    }
    
    // === SETTINGS HANDLERS ===
    
    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        Debug.Log($"Master Volume: {value}");
    }
    
    private void OnMusicVolumeChanged(float value)
    {
   
        Debug.Log($"Music Volume: {value}");
    }
    
    private void OnSFXVolumeChanged(float value)
    {
       
        Debug.Log($"Voice Volume: {value}");
    }
    
    private void OnResolutionChanged(int index)
    {
     
        Debug.Log($"Resolution changed to index: {index}");
    }
    
    // === SAVE/LOAD SETTINGS ===
    
    private void SaveSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        
        if (voiceVolumeSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", voiceVolumeSlider.value);
        
       
        
        PlayerPrefs.Save();
        Debug.Log("Settings saved");
    }
    
    private void LoadSettings()
    {
        if (masterVolumeSlider != null)
        {
            float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
            masterVolumeSlider.value = masterVolume;
            AudioListener.volume = masterVolume;
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        }
        
        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        }
        
       
        
        Debug.Log("Settings loaded");
    }
    
    // === PUBLIC PROPERTIES ===
    
    public bool IsSettingsOpen => isSettingsOpen;
}
