using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class LetterLogic : MonoBehaviour
{
    [SerializeField] private GameObject letterPanel; 
    [SerializeField] private MonoBehaviour playerController; 

    private bool canCloseLetter = true;

    public bool CanOpenExternal = false;

    private void Update()
    {
        if (CanOpenExternal == false)
            return;
        // Handle TAB input
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Debug.Log("=== TAB PRESSED ===");

                // If letter is already open, close it
                if (letterPanel.activeInHierarchy)
                {
                    if (canCloseLetter)
                    {
                        Debug.Log("Closing letter with TAB");
                        ToggleLetterPanel(false);
                    }
                    return; // Exit early to prevent opening logic
                }

                // If letter is closed, try to open it
                bool canOpen = CanOpenLetter();
                Debug.Log($"CanOpenLetter result: {canOpen}");

                if (canOpen)
                {
                    Debug.Log("Opening letter with TAB");
                    ToggleLetterPanel(true);
                }
                else
                {
                    Debug.Log("Cannot open letter - blocked by CanOpenLetter()");
                }
            }

        // Handle ESC input (only for closing)
        if (letterPanel.activeInHierarchy && canCloseLetter && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Closing letter with ESC");
            ToggleLetterPanel(false);
        }
    }

    private bool CanOpenLetter()
    {
        // Only block during dialogue, allow everywhere else
        var dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        return dialogueRunner == null || !dialogueRunner.IsDialogueRunning;
    }

    public void ToggleLetterPanel(bool? forceState = null)
    {
        bool isActive = forceState ?? !letterPanel.activeSelf;
        letterPanel.SetActive(isActive);

        // Lock/unlock player controls
        if (playerController != null)
            playerController.enabled = !isActive;

        // Show/hide cursor
        Cursor.visible = isActive;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        
        Debug.Log($"Letter panel toggled: {isActive}");
    }

    public void SetCanCloseLetter(bool canClose)
    {
        canCloseLetter = canClose;
    }
}
