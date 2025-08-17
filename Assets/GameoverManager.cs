using Unity.VisualScripting;
using UnityEngine;

public class GameoverManager : MonoBehaviour
{
    [SerializeField] private GameObject CreditsScreen;
    
    [Header("Ending Objects")]
    [SerializeField] private GameObject goodEndingObject;
    [SerializeField] private GameObject badEndingObject;
  

    void Start()
    {
        // Determina qual ending mostrar baseado nas escolhas
        DetermineEnding();
        

    }
    
    private void DetermineEnding()
    {
        // Lê as escolhas diretamente dos PlayerPrefs
        int goodChoices = PlayerPrefs.GetInt("GoodChoices", 0);
        int badChoices = PlayerPrefs.GetInt("BadChoices", 0);
        
        Debug.Log($"Reading choices - Good: {goodChoices}, Bad: {badChoices}");
        
        // Desativa todos primeiro
        if (goodEndingObject != null) goodEndingObject.SetActive(false);
        if (badEndingObject != null) badEndingObject.SetActive(false);
        
        // Ativa o apropriado baseado nas escolhas
        if (goodChoices > badChoices)
        {
            if (goodEndingObject != null) 
            {
                goodEndingObject.SetActive(true);
                Debug.Log("Showing Good Ending");
            }
        }
        else if (badChoices > goodChoices)
        {
            if (badEndingObject != null) 
            {
                badEndingObject.SetActive(true);
                Debug.Log("Showing Bad Ending");
            }
        }
        // Se quiser adicionar um neutral ending para empates
    }

    public void CloseCredits()
    {
        CreditsScreen.gameObject.SetActive(false);
    }

    public void OpenCredits()
    {
        CreditsScreen.gameObject.SetActive(true); 
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
