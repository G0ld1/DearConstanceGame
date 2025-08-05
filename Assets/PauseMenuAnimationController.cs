using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuAnimationController : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button saveExitButton;

    [Header("Panel")]
    [SerializeField] private GameObject PauseMenu;
    
    [Header("Animation")]
    [SerializeField] private Animator menuAnimator;
    
    // Trigger names (devem match os do Animator)
    
    private const string TO_SETTINGS_TRIGGER = "ToSettings";
    private const string TO_SAVE_EXIT_TRIGGER = "ToSaveExit";

    //State tracking
    private enum MenuState {Settings,SaveExit}
    private MenuState currentState = MenuState.Settings;
   
    
    private bool hasInitialized = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ShowPauseMenu()
    {
        PauseMenu.SetActive(true);
        
        if (!hasInitialized)
        {
            SetupHoverEvents();
            hasInitialized = true;
        }
        
        
        Debug.Log("Pause menu opened - playing initial animation");
    }
    
    private void SetupHoverEvents()
    {
        // Settings button - volta ao default
        AddHoverEvent(settingsButton, MenuState.Settings);
        
        // Save & Exit button - transição para SaveExit
        AddHoverEvent(saveExitButton, MenuState.SaveExit);
        
      
    }
    
    private void AddHoverEvent(Button button, MenuState targetState)
    {
        if (button == null) return;
        
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
        
        // Só precisas do PointerEnter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => {
            TransitionToState(targetState);
        });
        
        trigger.triggers.Add(enterEntry);
    }


    private void TransitionToState(MenuState newState)
    {
        // Só faz transição se o estado for diferente
        if (currentState == newState)
        {
            Debug.Log($"Already in {newState} state - ignoring transition");
            return;
        }
        
        Debug.Log($"Transitioning from {currentState} to {newState}");
        
        string triggerName = GetTriggerForState(newState);
        if (!string.IsNullOrEmpty(triggerName))
        {
            menuAnimator.SetTrigger(triggerName);
            currentState = newState;
        }
    }

        private string GetTriggerForState(MenuState state)
    {
        switch (state)
        {
            case MenuState.Settings:
                return TO_SETTINGS_TRIGGER;
            case MenuState.SaveExit:
                return TO_SAVE_EXIT_TRIGGER;
            default:
                return "";
        }
    }


      
    public void SetButtonsInteractable(bool interactable)
    {
        if (settingsButton != null)
            settingsButton.gameObject.SetActive(interactable);
        
        if (saveExitButton != null)
            saveExitButton.gameObject.SetActive(interactable);
        
        Debug.Log($"Pause menu buttons {(interactable ? "enabled" : "disabled")}");
    }
    

    
    // Métodos públicos se quiseres chamar de outros lugares
    public void OnSettingsHover() => menuAnimator.SetTrigger(TO_SETTINGS_TRIGGER);
    public void OnSaveExitHover() => menuAnimator.SetTrigger(TO_SAVE_EXIT_TRIGGER);
   
}
