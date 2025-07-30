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

    [SerializeField] private GameObject[] collectibleObjects; // Objetos para encontrar
    
    [Header("Narrative System")]
    [SerializeField] private DialogueRunner dialogueRunner;
    
    [Header("UI")]
    [SerializeField] private GameObject letterPanel;
    [SerializeField] private GameObject objectiveUI;
    [SerializeField] private TMPro.TextMeshProUGUI objectiveText;

    // Progress tracking
    private HashSet<string> foundObjects = new HashSet<string>();
    private int totalObjectsToFind;

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
        Debug.Log("=== InitializeGame START ===");
        
        // Find references if not assigned
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerMovementScript>();
            Debug.Log($"Found PlayerMovementScript: {(playerController != null ? "YES" : "NO")}");
        }
        
        // Find memory objects automatically
        InteractableStoryObject[] allStoryObjects = FindObjectsByType<InteractableStoryObject>(FindObjectsSortMode.None);
        Debug.Log($"Found {allStoryObjects.Length} InteractableStoryObjects");

        List<GameObject> memoryObjects = new List<GameObject>();
        foreach (var storyObj in allStoryObjects)
        {
            // Para já, adiciona todos os story objects como coletáveis
            memoryObjects.Add(storyObj.gameObject);
            Debug.Log($"Added {storyObj.name} as collectible");
        }

        collectibleObjects = memoryObjects.ToArray();
        totalObjectsToFind = collectibleObjects.Length;
        
        Debug.Log($"Total objects to find: {totalObjectsToFind}");
        
        // Start game
        Debug.Log("Changing to WakingUp state...");
        ChangeGameState(GameState.WakingUp);
        
        Debug.Log("=== InitializeGame END ===");
    }

    public void ChangeGameState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        Debug.Log($"Game State: {previousState} → {newState}");

        // Handle state transitions
        HandleStateExit(previousState);
        HandleStateEnter(newState);

        OnGameStateChanged?.Invoke(newState);
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
        // Player acorda no quarto
        Debug.Log("=== HandleWakingUpState START ===");
        
        // Debug player spawn
        if (Player != null && playerSpawnPoint != null)
        {
            Debug.Log($"Moving player from {Player.transform.position} to {playerSpawnPoint.transform.position}");
            Player.transform.position = playerSpawnPoint.transform.position;
            Debug.Log($"Player position after move: {Player.transform.position}");
        }
        else
        {
            Debug.LogError($"Player reference: {(Player != null ? "OK" : "NULL")}, SpawnPoint: {(playerSpawnPoint != null ? "OK" : "NULL")}");
        }
        
        // Debug player control
        Debug.Log("Enabling player control...");
        EnablePlayerControl();
        
        // Debug doors
        Debug.Log("Closing all doors...");
        CloseAllDoors();

        BRDoor.SetLocked(false);
        
        // Debug interactions
        Debug.Log("Disabling all interactions...");
        DisableAllInteractions();

        
              
        // Debug dialogue
        if (dialogueRunner != null)
        {
            Debug.Log("Starting Intro dialogue...");
            dialogueRunner.StartDialogue("Intro");
        }
        else
        {
            Debug.LogError("DialogueRunner reference is NULL!");
        }
        
        Debug.Log("Updating objective...");
        UpdateObjective("Go talk to Constance...");
        
        Debug.Log("=== HandleWakingUpState END ===");
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
        
        UpdateObjective("Go to your study...");
    }

    private void HandleReturningToStudyState()
    {
        // Frank vai para o estúdio
        DisableAllInteractions();
        
        // Tocar diálogo do estúdio
        if (dialogueRunner != null)
            dialogueRunner.StartDialogue("Study");
        
        UpdateObjective("Listen to Frank's thoughts...");
    }

    private void HandleExploringHouseState()
    {
        // Agora o jogador pode explorar livremente
        EnablePlayerControl();
        OpenAllDoors();
        EnableAllInteractions();
        
        // Mostrar objetos coletáveis
        foreach (GameObject obj in collectibleObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }
        
        UpdateObjective($"Explore the house and find memories");
    }

    private void HandleDeliveringLetterState()
    {
        // Jogador pode entregar a carta
        DisableAllInteractions();
        
        // Só pode interagir com a porta da Constance
        if (constanceDoor != null)
            constanceDoor.SetInteractable(true);
        
        UpdateObjective("Deliver the letter to Constance...");
    }

    private void HandleGameEndedState()
    {
        DisablePlayerControl();
        // Handle endings...
    }

    // Door Management
    private void CloseAllDoors()
    {
        Debug.Log($"CloseAllDoors called. Doors array length: {(allDoors != null ? allDoors.Length : 0)}");

        if (allDoors != null)
        {
            foreach (Door door in allDoors)
            {
                if (door != null)
                {
                    door.SetLocked(true);
                    Debug.Log($"Locked door: {door.name}");
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

    // Interaction Management
    private void EnableAllInteractions()
    {
        InteractableStoryObject[] storyObjects = FindObjectsByType<InteractableStoryObject>(FindObjectsSortMode.None);
        foreach (var storyObject in storyObjects)
        {
            storyObject.gameObject.SetActive(true);
        }
    }



    private void DisableAllInteractions()
    {
        InteractableStoryObject[] storyObjects = FindObjectsByType<InteractableStoryObject>(FindObjectsSortMode.None);
        foreach (var storyObject in storyObjects)
        {
            storyObject.gameObject.SetActive(false);
        }
    }


    // Player Control
    private void EnablePlayerControl()
    {
        Debug.Log($"EnablePlayerControl called. PlayerController: {(playerController != null ? "OK" : "NULL")}");
    
        if (playerController != null)
        {
            playerController.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log("Player control enabled successfully");
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
        Debug.Log($"UpdateObjective called with: {newObjective}");
    
        if (objectiveText != null)
        {
            objectiveText.text = newObjective;
            Debug.Log("Objective text updated successfully");
        }
        else
        {
            Debug.LogWarning("objectiveText is null - objective not displayed");
        }
    
        OnObjectiveUpdated?.Invoke(newObjective);
        Debug.Log($"Objective: {newObjective}");
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
            Debug.Log($"Found object: {objectName} ({foundObjects.Count}/{totalObjectsToFind})");
            
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
            // Trigger ending dialogue/cutscene
        }
    }

    // Getters
    public GameState GetCurrentState() => currentState;
    public bool CanInteract() => currentState == GameState.ExploringHouse;
    public bool CanMoveFreelyBetweenRooms() => currentState == GameState.ExploringHouse;
    public int GetFoundObjectsCount() => foundObjects.Count;
    public int GetTotalObjectsCount() => totalObjectsToFind;

    // Debug
    private void OnGUI()
    {
        if (Application.isEditor)
        {
            GUI.Box(new Rect(10, 10, 300, 120), 
                $"State: {currentState}\n" +
                $"Objects: {foundObjects.Count}/{totalObjectsToFind}\n" +
                $"Can Interact: {CanInteract()}\n" +
                $"Can Move Freely: {CanMoveFreelyBetweenRooms()}");
        }
    }
}

// Enhanced Game States
public enum GameState
{
    None,
    WakingUp,           // Frank acorda, ouve piano, vai para Constance
    GoingToConstance,   // Frank foi para sala, não consegue falar, volta para estúdio
    ReturningToStudy,   // Frank no estúdio, decide escrever carta
    ExploringHouse,     // Frank explora casa procurando memórias
    DeliveringLetter,   // Frank pode entregar a carta
    GameEnded           // Jogo terminou
}
