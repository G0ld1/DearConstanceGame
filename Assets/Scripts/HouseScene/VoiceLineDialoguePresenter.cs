using System.Threading;
using UnityEngine;
using Yarn.Unity;

public class VoiceLineDialoguePresenter : DialoguePresenterBase
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string voiceLinesPath = "VoiceLines/"; // Resources folder path
    
    private string currentPlayingLineID;
    
    void Start()
    {
        Debug.Log("VoiceLineDialoguePresenter: Started");
    }

    public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        Debug.Log("VoiceLineDialoguePresenter: RunOptionsAsync called");
        return YarnTask<DialogueOption?>.FromResult(null);
    }
    
    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        Debug.Log($"VoiceLineDialoguePresenter: RunLineAsync called with line ID: {line.TextID}");
        
        // Get the line ID
        string lineID = line.TextID;
        
        // Play voice line for this line ID
        PlayVoiceLineForID(lineID);
        
        // Wait until the line is cancelled (by other presenters or user input)
        await YarnTask.WaitUntilCanceled(token.NextLineToken).SuppressCancellationThrow();
    }
    
    public override YarnTask OnDialogueStartedAsync()
    {
        Debug.Log("VoiceLineDialoguePresenter: OnDialogueStartedAsync called");
        return YarnTask.CompletedTask;
    }
    
    public override YarnTask OnDialogueCompleteAsync()
    {
        Debug.Log("VoiceLineDialoguePresenter: OnDialogueCompleteAsync called");
        
        // Stop any playing voice line when dialogue ends
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        currentPlayingLineID = null;
        return YarnTask.CompletedTask;
    }
    
    private void PlayVoiceLineForID(string lineID)
    {
        Debug.Log($"VoiceLineDialoguePresenter: PlayVoiceLineForID called with: {lineID}");
        
        // Remove "line:" prefix if it exists
        string cleanLineID = lineID;
        if (lineID.StartsWith("line:"))
        {
            cleanLineID = lineID.Substring(5); // Remove "line:" (5 characters)
        }
        
        Debug.Log($"VoiceLineDialoguePresenter: Cleaned line ID: {cleanLineID}");
        
        // Don't replay if it's the same line
        if (currentPlayingLineID == cleanLineID)
        {
            Debug.Log($"VoiceLineDialoguePresenter: Already playing this line, skipping");
            return;
        }
            
        currentPlayingLineID = cleanLineID;
        
        // Load and play audio clip based on cleaned line ID
        string fullPath = voiceLinesPath + cleanLineID;
        Debug.Log($"VoiceLineDialoguePresenter: Trying to load audio from: {fullPath}");
        
        AudioClip voiceClip = Resources.Load<AudioClip>(fullPath);
        
        if (voiceClip != null)
        {
            Debug.Log($"VoiceLineDialoguePresenter: Audio clip loaded successfully: {voiceClip.name}");
            audioSource.clip = voiceClip;
            audioSource.Play();
            Debug.Log($"Playing voice line: {cleanLineID}");
        }
        else
        {
            Debug.LogWarning($"Voice line not found for ID: {cleanLineID} at path: {fullPath}");
        }
    }

    public void SetVoiceVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
