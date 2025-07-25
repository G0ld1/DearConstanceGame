using UnityEngine;
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
            manager.letterSections[sectionKey].content = content;
            manager.letterSections[sectionKey].isWritten = true;
            manager.UpdateLetterUI();
            Debug.Log($"Successfully added content to section: {sectionKey}");
        }
        else
        {
            Debug.LogWarning($"Section '{sectionKey}' not found. Available sections: {string.Join(", ", manager.letterSections.Keys)}");
        }
    }
    
    private void UpdateLetterUI()
    {
        string fullLetter = "Dear Constance,\n\n"; // Letter header
        
        // Order sections by sectionOrder
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
                fullLetter += "...\n\n"; // Placeholder for unwritten sections
            }
        }

        fullLetter += "\nFrank"; // Letter signature
        letterUI.text = fullLetter;

        // Scroll to top (1) or bottom (0)
        if (letterScrollRect != null)
            letterScrollRect.verticalNormalizedPosition = 1f; // 1 = top, 0 = bottom
    }

   
}
