using UnityEngine;

public class LetterLogic : MonoBehaviour
{

    [SerializeField] private GameObject letterPanel; // Assign your Scroll View panel here
    [SerializeField] private MonoBehaviour playerController; // Assign your player movement script here

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isActive = !letterPanel.activeSelf;
            letterPanel.SetActive(isActive);

            // Lock/unlock player controls
            if (playerController != null)
                playerController.enabled = !isActive;

            // Show/hide cursor
            Cursor.visible = isActive;
            Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
