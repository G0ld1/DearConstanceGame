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
        public int sectionOrder; 
    }

    [SerializeField] private TMPro.TextMeshProUGUI letterUI;
    [SerializeField] private List<LetterSection> availableSections; 
    private Dictionary<string, LetterSection> letterSections = new Dictionary<string, LetterSection>();

    [SerializeField] private UnityEngine.UI.ScrollRect letterScrollRect; 
    [SerializeField] private GameObject letterPanel; 

    private Coroutine typewriterCoroutine;
    private Queue<string> pendingSections = new Queue<string>(); // Fila para secções pendentes
    private bool isTypewriting = false; // Flag para controlar se está a escrever

    public static int TotalGoodChoices { get; private set; } = 0;
    public static int TotalBadChoices { get; private set; } = 0;

    private void Awake()
    {
        // Initialize sections from inspector list
        letterSections = new Dictionary<string, LetterSection>();
        foreach (var section in availableSections)
        {
            letterSections.Add(section.sectionTitle.ToLower(), section);
        }
        
        UpdateLetterUI();
    }

    [YarnCommand("write_to_letter")]
    public static void WriteToLetter(string sectionKey, string content)
    {
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

                // Ativa o painel da carta
                if (manager.letterPanel != null)
                    manager.letterPanel.SetActive(true);

                var letterLogic = FindFirstObjectByType<LetterLogic>();
                if (letterLogic != null)
                    letterLogic.ToggleLetterPanel(true);

                // Em vez de iniciar imediatamente, adiciona à fila
                manager.QueueSectionForTypewriter(sectionKey);
                
                Debug.Log($"Successfully queued content for section: {sectionKey}");
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

    private void QueueSectionForTypewriter(string sectionKey)
    {
        pendingSections.Enqueue(sectionKey);
        
        // Se não está a escrever, inicia o processo
        if (!isTypewriting)
        {
            StartCoroutine(ProcessTypewriterQueue());
        }
    }

    private IEnumerator ProcessTypewriterQueue()
    {
        isTypewriting = true;

        while (pendingSections.Count > 0)
        {
            string sectionKey = pendingSections.Dequeue();
            
            // Para a corrotina anterior se existir
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            
            // Inicia nova corrotina e espera que termine
            typewriterCoroutine = StartCoroutine(TypewriterEffect(sectionKey));
            yield return typewriterCoroutine;
            
            // Pequena pausa entre secções se houver mais na fila
            if (pendingSections.Count > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        isTypewriting = false;
    }

    [YarnCommand("check_if_all_choices_made")]
    public static void CheckIfAllChoicesMade()
    {
        Debug.Log("A verificar se ta tudo");
        
        // Espera que termine de escrever antes de verificar
        var manager = FindFirstObjectByType<LetterManager>();
        if (manager != null)
        {
            manager.StartCoroutine(manager.WaitAndCheckChoices());
        }
    }

    private IEnumerator WaitAndCheckChoices()
    {
        // Espera que termine de escrever
        while (isTypewriting || pendingSections.Count > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // Agora verifica as escolhas
        DialogueRunner dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue("CheckAllChoicesMade");
        }
    }

    private IEnumerator TypewriterEffect(string newSectionKey)
    {
        string fullLetter = "Dear Constance,\n\n";
        var orderedSections = new List<LetterSection>(letterSections.Values);
        orderedSections.Sort((a, b) => a.sectionOrder.CompareTo(b.sectionOrder));

        var letterLogic = FindFirstObjectByType<LetterLogic>();
        if (letterLogic != null)
            letterLogic.SetCanCloseLetter(false);

        foreach (var section in orderedSections)
        {
            if (section.isWritten && section.sectionTitle.ToLower() != newSectionKey)
            {
                fullLetter += $"{section.content}\n\n";
            }
            else if (section.isWritten && section.sectionTitle.ToLower() == newSectionKey)
            {
                string content = section.content;
                for (int i = 0; i <= content.Length; i++)
                {
                    letterUI.text = fullLetter + content.Substring(0, i) + "\n\n...";
                    yield return new WaitForSeconds(0.01f);
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

        if (letterLogic != null)
            letterLogic.SetCanCloseLetter(true);
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
    }

    [YarnCommand("save_choices")]
    public static void SaveChoices(int goodChoices, int badChoices)
    {
        PlayerPrefs.SetInt("GoodChoices", goodChoices);
        PlayerPrefs.SetInt("BadChoices", badChoices);
        PlayerPrefs.Save();
        Debug.Log($"Choices saved - Good: {goodChoices}, Bad: {badChoices}");
    }

}
