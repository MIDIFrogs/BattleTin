using UnityEngine;
using System.Collections;

public enum MenuState
{
    MainMenu,
    Settings,
    Tutorial,
    SearchingMatch
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [SerializeField] private MenuState currentState = MenuState.MainMenu;
    public MenuState CurrentState => currentState;

    [Header("UI References")]
    [SerializeField] private GameObject settingsPopup;
    [SerializeField] private GameObject tutorialPopup;
    [SerializeField] private IconDisplayManager iconManager;
    [SerializeField] private MatchmakingController matchmakingManager;

    [Header("Buttons")]
    [SerializeField] private GameObject playButton; 
    [SerializeField] private GameObject cancelSearchButton; 

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetState(MenuState.MainMenu);
        if (SettingsManager.Instance != null)
        {
            
        }
    }

    public void SetState(MenuState newState)
    {
        if (currentState == newState || isTransitioning) return;

        isTransitioning = true;
        currentState = newState;

        UpdateUI();

        StartCoroutine(ResetTransitionFlag());
    }

    private IEnumerator ResetTransitionFlag()
    {
        yield return null;
        isTransitioning = false;
    }

    private void UpdateUI()
    {
        if (settingsPopup != null)
            settingsPopup.SetActive(currentState == MenuState.Settings);

        if (tutorialPopup != null)
            tutorialPopup.SetActive(currentState == MenuState.Tutorial);

        bool isSearching = currentState == MenuState.SearchingMatch;

        if (playButton != null)
            playButton.SetActive(!isSearching);

        if (cancelSearchButton != null)
            cancelSearchButton.SetActive(isSearching);
    }

    public void StartGame()
    {
        if (currentState == MenuState.SearchingMatch || isTransitioning) return;

        SetState(MenuState.SearchingMatch);

        if (matchmakingManager != null)
            matchmakingManager.StartSearch();

        if (iconManager != null)
            iconManager.ShowSearchIcon();
    }

    public void OpenSettings()
    {
        if (isTransitioning) return;
        SetState(MenuState.Settings);

        if (iconManager != null)
            iconManager.ShowSettingsIcon();
    }

    public void OpenTutorial()
    {
        if (isTransitioning) return;
        SetState(MenuState.Tutorial);

        if (iconManager != null)
            iconManager.ShowTutorialIcon();
    }

    public void ClosePopup()
    {
        if (isTransitioning) return;
        SetState(MenuState.MainMenu);

        if (iconManager != null)
            iconManager.ShowPlayIcon();
    }

    public void QuitGame()
    {

    }

    public void OnMatchFound()
    {
        if (isTransitioning) return;

        SetState(MenuState.MainMenu);
        if (iconManager != null)
            iconManager.ShowPlayIcon();

        // Здесь можно загрузить сцену игры
        Debug.Log("Starting game...");
    }

    public void CancelMatchmaking()
    {
        if (matchmakingManager != null)
            matchmakingManager.CancelSearch();
    }
}