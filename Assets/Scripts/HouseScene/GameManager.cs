using UnityEngine;
using System;
using System.Collections.Generic;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.None;

    [Header("Player References")]
    [SerializeField] private Transform Player;
    [SerializeField] private PlayerMovementScript playerController;
    [SerializeField] private Transform playerSpawnPoint; // Quarto inicial

    [Header("Door Management")]
    [SerializeField] private Door[] allDoors;
    [SerializeField] private Door constanceDoor; // Porta para sala da Constance
    [SerializeField] private Door studioDoor; // Porta para o estúdio

    [SerializeField] private Door BRDoor; //porta do quarto
    [SerializeField] private Door LRDoor; //porta da sala

    [Header("Interaction System")]

    [SerializeField] private GameObject[] collectibleObjects;

    [Header("Narrative System")]
    [SerializeField] private DialogueRunner dialogueRunner;

    [Header("UI")]

    [SerializeField] private GameObject objectiveUI;
    [SerializeField] private TMPro.TextMeshProUGUI objectiveText;
    [SerializeField] private GameObject gameOverScreen;

    [Header("Debug Tools")]
    [SerializeField] private bool enableDebugTools = true;

    // Progress tracking
    private HashSet<string> foundObjects = new HashSet<string>();
    private int totalObjectsToFind;

    [SerializeField] private LetterLogic letterLogic;
    [SerializeField] private LetterManager letterManager;

    // Events
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<string> OnObjectiveUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {



        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerMovementScript>();
        }


        InteractableStoryObject[] allStoryObjects = FindObjectsByType<InteractableStoryObject>(FindObjectsSortMode.None);

        List<GameObject> memoryObjects = new List<GameObject>();
        foreach (var storyObj in allStoryObjects)
        {

            memoryObjects.Add(storyObj.gameObject);
        }

        collectibleObjects = memoryObjects.ToArray();
        totalObjectsToFind = collectibleObjects.Length;


        // Start game
        ChangeGameState(GameState.WakingUp);

    }

    public void ChangeGameState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;


        // Handle state transitions
        HandleStateExit(previousState);
        HandleStateEnter(newState);

        OnGameStateChanged?.Invoke(newState);
    }

    [YarnCommand("complete_study")]
    public static void CompleteStudyDialogue()
    {
        if (Instance != null)
        {
            Instance.OnStudyDialogueComplete();
        }
    }


    private void HandleStateExit(GameState exitingState)
    {
        // Cleanup previous state
        switch (exitingState)
        {
            case GameState.WakingUp:
                break;
            case GameState.GoingToConstance:
                break;
            case GameState.ReturningToStudy:
                break;
            case GameState.ExploringHouse:
                break;
            case GameState.DeliveringLetter:
                break;
        }
    }

    private void HandleStateEnter(GameState enteringState)
    {
        switch (enteringState)
        {
            case GameState.WakingUp:
                HandleWakingUpState();
                break;

            case GameState.GoingToConstance:
                HandleGoingToConstanceState();
                break;

            case GameState.ReturningToStudy:
                HandleReturningToStudyState();
                break;

            case GameState.ExploringHouse:
                HandleExploringHouseState();
                break;

            case GameState.DeliveringLetter:
                HandleDeliveringLetterState();
                break;

            case GameState.GameEnded:
                HandleGameEndedState();
                break;
        }
    }

    private void HandleWakingUpState()
    {



        if (Player != null && playerSpawnPoint != null)
        {

            Player.transform.position = playerSpawnPoint.transform.position;

        }
        else
        {
        }

        
        EnablePlayerControl();


        CloseAllDoors();

        BRDoor.SetLocked(false);



        DisableAllInteractions();




        if (dialogueRunner != null)
        {

            dialogueRunner.StartDialogue("Intro");
        }
        else
        {
            Debug.LogError("DialogueRunner reference is NULL!");
        }


        UpdateObjective("Try to talk to Constance");


    }

    private void HandleGoingToConstanceState()
    {
        // Jogador foi para a sala da Constance
        DisableAllInteractions();

        // Tocar diálogo "CantDoIt"
        if (dialogueRunner != null)
            dialogueRunner.StartDialogue("CantDoIt");

        // Só a porta para o estúdio fica aberta
        CloseAllDoors();
        if (studioDoor != null)
            studioDoor.SetLocked(false);

        UpdateObjective("Go to your study");
    }

    private void HandleReturningToStudyState()
    {
        // Frank vai para o estúdio
        DisableAllInteractions();

        // Tocar diálogo do estúdio
        if (dialogueRunner != null)
            dialogueRunner.StartDialogue("Study");

        UpdateObjective("Listen to Frank's troubles");
    }

    private void HandleExploringHouseState()
    {
        // Agora o jogador pode explorar livremente
        EnablePlayerControl();
        OpenAllDoors();
        EnableAllInteractions();


        foreach (GameObject obj in collectibleObjects)
        {
            if (obj != null)
            {
                var interactable = obj.GetComponent<InteractableStoryObject>();
                if (interactable != null)
                {
                    interactable.enabled = true;
                }
            }
        }

        UpdateObjective($"Explore the house and find memories");

        letterLogic.CanOpenExternal = true;
    }

    private void HandleDeliveringLetterState()
    {

        DisableAllInteractions();

        ObjectiveCardUI.Instance.ShowObjectiveCard("Deliver the letter to Constance.");
       
        if (constanceDoor != null)
            constanceDoor.SetInteractable(true);

    }

    private void HandleGameEndedState()
    {
        DisablePlayerControl();

        // Ativar o Game Over Screen
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
        else
        {
            Debug.LogError("Game Over Screen GameObject not assigned!");
        }


    }


    private void CloseAllDoors()
    {

        if (allDoors != null)
        {
            foreach (Door door in allDoors)
            {
                if (door != null)
                {
                    door.SetLocked(true);
                }
                else
                {
                    Debug.LogWarning("Found null door in allDoors array");
                }
            }
        }
        else
        {
            Debug.LogError("allDoors array is null!");
        }

        LRDoor.SetLocked(false);

    }

    private void OpenAllDoors()
    {
        foreach (Door door in allDoors)
        {
            if (door != null)
                door.SetLocked(false);
        }
    }


    private void EnableAllInteractions()
    {
        InteractableStoryObject[] storyObjects = FindObjectsByType<InteractableStoryObject>(FindObjectsSortMode.None);
        foreach (var storyObject in storyObjects)
        {
            storyObject.enabled = true;
        }
    }

    private void DisableAllInteractions()
    {
        InteractableStoryObject[] storyObjects = FindObjectsByType<InteractableStoryObject>(FindObjectsSortMode.None);
        foreach (var storyObject in storyObjects)
        {
            storyObject.enabled = false;
        }
    }



    private void EnablePlayerControl()
    {

        if (playerController != null)
        {
            playerController.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Debug.LogError("Cannot enable player control - playerController is null!");
        }
    }

    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // Objective System
    private void UpdateObjective(string newObjective)
    {

        if (objectiveText != null)
        {
            objectiveText.text = newObjective;

        }
        else
        {
            Debug.LogWarning("objectiveText is null - objective not displayed");
        }


    }

    // Public methods for triggers and interactions
    public void OnReachedConstance()
    {
        if (currentState == GameState.WakingUp)
        {
            ChangeGameState(GameState.GoingToConstance);
        }
    }

    public void OnReachedStudy()
    {
        if (currentState == GameState.GoingToConstance)
        {
            ChangeGameState(GameState.ReturningToStudy);
        }
    }

    public void OnStudyDialogueComplete()
    {
        if (currentState == GameState.ReturningToStudy)
        {
            ChangeGameState(GameState.ExploringHouse);
        }
    }

    public void OnObjectFound(string objectName)
    {
        if (currentState != GameState.ExploringHouse) return;

        if (!foundObjects.Contains(objectName))
        {
            foundObjects.Add(objectName);

            UpdateObjective($"Explore the house and find memories ({foundObjects.Count}/{totalObjectsToFind})");

            if (foundObjects.Count >= totalObjectsToFind)
            {
                ChangeGameState(GameState.DeliveringLetter);
            }
        }
    }

    public void OnLetterDelivered()
    {
        if (currentState == GameState.DeliveringLetter)
        {
            ChangeGameState(GameState.GameEnded);

        }
    }

    // Debug methods com botões no Inspector
    [ContextMenu("Debug: Force WakingUp")]
    public void Debug_ForceWakingUp() => ChangeGameState(GameState.WakingUp);

    [ContextMenu("Debug: Force GoingToConstance")]
    public void Debug_ForceGoingToConstance() => ChangeGameState(GameState.GoingToConstance);

    [ContextMenu("Debug: Force ReturningToStudy")]
    public void Debug_ForceReturningToStudy() => ChangeGameState(GameState.ReturningToStudy);

    [ContextMenu("Debug: Force ExploringHouse")]
    public void Debug_ForceExploringHouse() => ChangeGameState(GameState.ExploringHouse);

    [ContextMenu("Debug: Force DeliveringLetter")]
    public void Debug_ForceDeliveringLetter() => ChangeGameState(GameState.DeliveringLetter);

    [ContextMenu("Debug: Force GameEnded")]
    public void Debug_ForceGameEnded() => ChangeGameState(GameState.GameEnded);

    [ContextMenu("Debug: Add Random Memory")]
    public void Debug_AddRandomMemory()
    {
        string randomMemory = $"DebugMemory_{UnityEngine.Random.Range(1000, 9999)}";
        OnObjectFound(randomMemory);
    }

    [ContextMenu("Debug: Complete All Memories")]
    public void Debug_CompleteAllMemories()
    {
        for (int i = foundObjects.Count; i < totalObjectsToFind; i++)
        {
            OnObjectFound($"DebugMemory_{i}");
        }
    }

    // Getters
    public GameState GetCurrentState() => currentState;
    public bool CanInteract() => currentState == GameState.ExploringHouse;
    public bool CanMoveFreelyBetweenRooms() => currentState == GameState.ExploringHouse;
    public int GetFoundObjectsCount() => foundObjects.Count;
    public int GetTotalObjectsCount() => totalObjectsToFind;


}


// Enhanced Game States
public enum GameState
{
    None,
    WakingUp,           
    GoingToConstance,   
    ReturningToStudy,   
    ExploringHouse,    
    DeliveringLetter,   
    GameEnded           
}
