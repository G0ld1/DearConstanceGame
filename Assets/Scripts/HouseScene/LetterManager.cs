using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Yarn.Unity;

public class LetterManager : MonoBehaviour
{
    [System.Serializable]
    public class LetterSection
    {
        public string sectionTitle;
        public string content;
        public bool isWritten;
        public int sectionOrder; // To maintain letter structure
    }

    [SerializeField] private TMPro.TextMeshProUGUI letterUI;
    [SerializeField] private List<LetterSection> availableSections; // Configure in inspector
    private Dictionary<string, LetterSection> letterSections = new Dictionary<string, LetterSection>();

    [SerializeField] private UnityEngine.UI.ScrollRect letterScrollRect; // Assign in inspector
    [SerializeField] private GameObject letterPanel; // Assign your Scroll View panel here

    private Coroutine typewriterCoroutine;

    private void Awake()
    {
        // Initialize sections from inspector list
        letterSections = new Dictionary<string, LetterSection>();
        foreach (var section in availableSections)
        {
            letterSections.Add(section.sectionTitle.ToLower(), section);
        }
        
        // Initialize UI
        UpdateLetterUI();
    }

    [YarnCommand("write_to_letter")]
    public static void WriteToLetter(string sectionKey, string content)
    {
        // Find the LetterManager instance in the scene
        var manager = FindFirstObjectByType<LetterManager>();
        if (manager == null)
        {
            Debug.LogError("No LetterManager found in scene!");
            return;
        }
        sectionKey = sectionKey.ToLower();
        Debug.Log($"Attempting to add content to section: {sectionKey}");
        if (manager.letterSections.ContainsKey(sectionKey))
        {
            var section = manager.letterSections[sectionKey];
            if (!section.isWritten)
            {
                section.content = content;
                section.isWritten = true;

                // Show the letter panel
                if (manager.letterPanel != null)
                    manager.letterPanel.SetActive(true);

                // Lock player controls and show cursor using LetterLogic
                var letterLogic = FindFirstObjectByType<LetterLogic>();
                if (letterLogic != null)
                    letterLogic.ToggleLetterPanel(true);

                // Animate the new section
                if (manager.typewriterCoroutine != null)
                    manager.StopCoroutine(manager.typewriterCoroutine);
                manager.typewriterCoroutine = manager.StartCoroutine(manager.TypewriterEffect(sectionKey));
                
                Debug.Log($"Successfully added content to section: {sectionKey}");
            }
            else
            {
                Debug.Log($"Section '{sectionKey}' is already written and cannot be overwritten.");
            }
        }
        else
        {
            Debug.LogWarning($"Section '{sectionKey}' not found. Available sections: {string.Join(", ", manager.letterSections.Keys)}");
        }
    }

    private IEnumerator TypewriterEffect(string newSectionKey)
    {
        string fullLetter = "Dear Constance,\n\n";
        var orderedSections = new List<LetterSection>(letterSections.Values);
        orderedSections.Sort((a, b) => a.sectionOrder.CompareTo(b.sectionOrder));

        foreach (var section in orderedSections)
        {
            if (section.isWritten && section.sectionTitle.ToLower() != newSectionKey)
            {
                fullLetter += $"{section.content}\n\n";
            }
            else if (section.isWritten && section.sectionTitle.ToLower() == newSectionKey)
            {
                // Animate this section
                string content = section.content;
                for (int i = 0; i <= content.Length; i++)
                {
                    letterUI.text = fullLetter + content.Substring(0, i) + "\n\n...";
                    if (letterScrollRect != null)
                        letterScrollRect.verticalNormalizedPosition = 1f;
                    yield return new WaitForSeconds(0.05f); // Adjust speed as needed
                }
                fullLetter += content + "\n\n";
            }
            else
            {
                fullLetter += "...\n\n";
            }
        }

        fullLetter += "\nFrank";
        letterUI.text = fullLetter;
        if (letterScrollRect != null)
            letterScrollRect.verticalNormalizedPosition = 1f;
    }

    private void UpdateLetterUI()
    {
        string fullLetter = "Dear Constance,\n\n";
        var orderedSections = new List<LetterSection>(letterSections.Values);
        orderedSections.Sort((a, b) => a.sectionOrder.CompareTo(b.sectionOrder));

        foreach (var section in orderedSections)
        {
            if (section.isWritten)
            {
                fullLetter += $"{section.content}\n\n";
            }
            else
            {
                fullLetter += "...\n\n";
            }
        }

        fullLetter += "\nFrank";
        letterUI.text = fullLetter;
        if (letterScrollRect != null)
            letterScrollRect.verticalNormalizedPosition = 1f;
    }
}
