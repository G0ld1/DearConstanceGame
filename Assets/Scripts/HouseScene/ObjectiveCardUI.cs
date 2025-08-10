using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Yarn.Unity; // Adiciona esta linha

public class ObjectiveCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform cardTransform;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private CanvasGroup cardCanvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float slideInDuration = 0.6f;
    [SerializeField] private float displayDuration = 3.0f;
    [SerializeField] private float slideOutDuration = 0.5f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Position Settings")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-1417f, 404f); 
    [SerializeField] private Vector2 visiblePosition = new Vector2(-744f, 404f); 
    
    [Header("Fade Effect")]
    [SerializeField] private bool enableFadeEffect = true;
    
    private Coroutine currentAnimationCoroutine;
    private bool isAnimating = false;
    
    // === SINGLETON PATTERN ===
    
    public static ObjectiveCardUI Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple ObjectiveCardUI instances found! Destroying duplicate.");
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeCard();
        SubscribeToEvents();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeCard()
    {
        if (cardTransform == null)
            cardTransform = GetComponent<RectTransform>();
        
        if (cardCanvasGroup == null)
            cardCanvasGroup = GetComponent<CanvasGroup>();
        
        // Position inicial (escondido à esquerda)
        SetCardToHiddenState();
    }
    
    private void SetCardToHiddenState()
    {
        cardTransform.anchoredPosition = hiddenPosition;
        
        if (enableFadeEffect && cardCanvasGroup != null)
        {
            cardCanvasGroup.alpha = 0f;
        }
    }
    
    private void SubscribeToEvents()
    {
        GameManager.OnObjectiveUpdated += ShowObjectiveCard;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameManager.OnObjectiveUpdated -= ShowObjectiveCard;
    }
    
    public void ShowObjectiveCard(string newObjective)
    {
        Debug.Log($"ShowObjectiveCard called with: {newObjective}");
        
        // Para animação anterior se estiver a correr
        if (currentAnimationCoroutine != null)
        {
            Debug.Log("Stopping previous animation");
            StopCoroutine(currentAnimationCoroutine);
        }
        
        // Atualiza o texto
        if (objectiveText != null)
        {
            objectiveText.text = newObjective;
            Debug.Log($"Text updated to: {newObjective}");
        }
        else
        {
            Debug.LogError("ObjectiveText is NULL!");
        }
        
        // Inicia nova animação
        Debug.Log("Starting ObjectiveCardSequence");
        currentAnimationCoroutine = StartCoroutine(ObjectiveCardSequence());
    }

    private IEnumerator ObjectiveCardSequence()
    {
        Debug.Log("ObjectiveCardSequence started");
        isAnimating = true;
        
        // 1. Slide In
        Debug.Log("Starting SlideIn");
        yield return StartCoroutine(SlideIn());
        Debug.Log("SlideIn completed");
        
        // 2. Display (fica visível)
        Debug.Log($"Displaying for {displayDuration} seconds");
        yield return new WaitForSecondsRealtime(displayDuration);
        
        // 3. Slide Out
        Debug.Log("Starting SlideOut");
        yield return StartCoroutine(SlideOut());
        Debug.Log("SlideOut completed");
        
        isAnimating = false;
    }
    
    private IEnumerator SlideIn()
    {
        Debug.Log($"SlideIn: Moving from {hiddenPosition} to {visiblePosition}");
        
        float elapsed = 0f;
        
        Vector2 startPos = hiddenPosition;
        Vector2 endPos = visiblePosition;
        
        float startAlpha = enableFadeEffect ? 0f : 1f;
        float endAlpha = 1f;
        
        Debug.Log($"SlideIn duration: {slideInDuration}");
        
        while (elapsed < slideInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / slideInDuration;
            float curveValue = slideCurve.Evaluate(progress);
            
            // Position
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, curveValue);
            cardTransform.anchoredPosition = currentPos;
            
            // Debug a cada frame (remove depois)
            if (elapsed % 0.1f < Time.unscaledDeltaTime) // Log every ~0.1 seconds
            {
                Debug.Log($"SlideIn progress: {progress:F2}, position: {currentPos}");
            }
            
            // Fade
            if (enableFadeEffect && cardCanvasGroup != null)
            {
                cardCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            }
            
            yield return null;
        }
        
        // Ensure final values
        cardTransform.anchoredPosition = endPos;
        if (enableFadeEffect && cardCanvasGroup != null)
            cardCanvasGroup.alpha = endAlpha;
        
        Debug.Log($"SlideIn finished at position: {endPos}");
    }
    
    private IEnumerator SlideOut()
    {
        float elapsed = 0f;
        
        Vector2 startPos = visiblePosition;
        Vector2 endPos = hiddenPosition;
        
        float startAlpha = 1f;
        float endAlpha = enableFadeEffect ? 0f : 1f;
        
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideOutDuration;
            float curveValue = slideCurve.Evaluate(progress);
            
            // Position
            cardTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            // Fade
            if (enableFadeEffect && cardCanvasGroup != null)
            {
                cardCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            }
            
            yield return null;
        }
        
        // Final state
        SetCardToHiddenState();
    }
    
    // Public methods para controlar manualmente
    public void ForceHide()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        SetCardToHiddenState();
        isAnimating = false;
    }
    
    // Properties
    public bool IsAnimating => isAnimating;
    
    // Debug
    [ContextMenu("Test Objective Card")]
    private void TestObjectiveCard()
    {
        ShowObjectiveCard("Test: Find the mysterious object!");
    }
    
    // === YARN COMMANDS ===
    
    [YarnCommand("show_objective")]
    public static void ShowObjectiveCommand(string objectiveText)
    {
        if (Instance != null)
        {
            Debug.Log($"Yarn Command: show_objective called with '{objectiveText}'");
            Instance.ShowObjectiveCard(objectiveText);
        }
        else
        {
            Debug.LogError("ObjectiveCardUI Instance not found!");
        }
    }
    
    [YarnCommand("hide_objective")]
    public static void HideObjectiveCommand()
    {
        if (Instance != null)
        {
            Debug.Log("Yarn Command: hide_objective called");
            Instance.ForceHide();
        }
        else
        {
            Debug.LogError("ObjectiveCardUI Instance not found!");
        }
    }
    
    [YarnCommand("update_objective")]
    public static void UpdateObjectiveCommand(string newObjectiveText)
    {
        if (Instance != null)
        {
            Debug.Log($"Yarn Command: update_objective called with '{newObjectiveText}'");
            Instance.UpdateObjectiveText(newObjectiveText);
        }
        else
        {
            Debug.LogError("ObjectiveCardUI Instance not found!");
        }
    }
    
    // === NOVOS MÉTODOS PÚBLICOS ===
    
    public void UpdateObjectiveText(string newText)
    {
        if (objectiveText != null)
        {
            objectiveText.text = newText;
            Debug.Log($"Objective text updated to: {newText}");
        }
    }
    
    public void ShowObjectiveImmediate(string objective)
    {
        if (objectiveText != null)
        {
            objectiveText.text = objective;
        }
        
        cardTransform.anchoredPosition = visiblePosition;
        if (cardCanvasGroup != null)
            cardCanvasGroup.alpha = 1f;
        
        Debug.Log($"Objective shown immediately: {objective}");
    }
}
