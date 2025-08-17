using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Scene Management")]
    [SerializeField] private string gameSceneName = "SampleScene"; 
    
    [Header("Settings UI")]
    [SerializeField] private Slider masterVolumeSlider;
   
    [SerializeField] private Slider voiceVolumeSlider;

    [SerializeField] private Slider MainMenuMusicSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    [Header("Menu Audio")]
    [SerializeField] private AudioSource menuMusicAudioSource; 
    
    // Audio volume values
    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float voiceVolume = 1f;

    private float MMmusicVol = 0.1f;

    void Start()
    {
        // Certifica que só o menu principal está ativo
        ShowMainMenu();
        
        // Configura cursor para o menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Configura os settings
        SetupSettingsUI();
        LoadSettings();
        
    
    }



    // Métodos para os botões do Menu Principal
    public void StartGame()
    {
        Debug.Log("Starting game...");
        
        // Salva as configurações antes de trocar de scene
        SaveSettings();
        
        // Para a música do menu antes de trocar de scene
        if (menuMusicAudioSource != null)
        {
            menuMusicAudioSource.Stop();
        }
        
        // Carrega a scene do jogo
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void OpenSettings()
    {
        Debug.Log("Opening settings...");
        
        // Esconde menu principal e mostra settings
        settingsPanel.SetActive(true);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        // Salva as configurações antes de sair
        SaveSettings();
        
        // Funciona no build, não no editor
        Application.Quit();
        
        // Para testar no editor do Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // Métodos para o painel de Settings
    public void CloseSettings()
    {
        Debug.Log("Closing settings...");
        
        // Salva as configurações ao fechar
        SaveSettings();
        
        // Volta ao menu principal
        ShowMainMenu();
    }
    
    public void ApplySettings()
    {
        Debug.Log("Applying settings...");
        
        // Salva as configurações
        SaveSettings();
        
        // Volta ao menu principal
        ShowMainMenu();
    }
    
    // Método auxiliar para mostrar menu principal
    private void ShowMainMenu()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    // === CONFIGURAÇÃO DOS SETTINGS UI ===
    
    private void SetupSettingsUI()
    {
        // Setup audio sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
    
        
        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
        }

        if (MainMenuMusicSlider != null)
        {
            MainMenuMusicSlider.onValueChanged.AddListener(OnMainMenuVolumeChanged);
        }
        
        if (resolutionDropdown != null)
            {
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }
    }
    
    // === HANDLERS DOS SETTINGS ===
    
    private void OnMasterVolumeChanged(float value)
    {
        masterVolume = value;
       
        Debug.Log($"Master Volume: {value}");
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        musicVolume = value;
     
        Debug.Log($"Music Volume: {value}");
    }
    
    private void OnVoiceVolumeChanged(float value)
    {
        voiceVolume = value;
      
        Debug.Log($"Voice Volume: {value}");
    }

    private void OnMainMenuVolumeChanged(float value)
    {
        MMmusicVol = value;
        ApplyAudioSettings();
    }
    
    private void OnResolutionChanged(int index)
    {
        Debug.Log($"Resolution changed to index: {index}");
        // Salva a resolução escolhida
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }
    
    private void ApplyAudioSettings()
    {
         
        // Aplica o volume da música do menu
        if (menuMusicAudioSource != null)
        {
            menuMusicAudioSource.volume = MMmusicVol;
        }       
  
    }
    
    // === SAVE/LOAD SETTINGS ===
    
    private void SaveSettings()
    {
        // Usa as MESMAS chaves que o SettingsMenuController
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
        
        PlayerPrefs.Save();
        Debug.Log("Main Menu Settings saved");
    }
    
    private void LoadSettings()
    {
        // Carrega com os MESMOS valores padrão que o SettingsMenuController
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 0.8f);
        
        // Atualiza os sliders para refletir os valores carregados
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
        }
        
       
        
        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.value = voiceVolume;
        }
        
        // Carrega resolução se houver
        if (resolutionDropdown != null)
        {
            int savedResolution = PlayerPrefs.GetInt("ResolutionIndex", 0);
            resolutionDropdown.value = savedResolution;
        }
        
        // Aplica as configurações carregadas
        ApplyAudioSettings();
        
        Debug.Log("Main Menu Settings loaded");
    }
}
