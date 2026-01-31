using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MatchmakingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IconDisplayManager iconManager;
    [SerializeField] private Button cancelButton; 

    [Header("Settings")]
    [SerializeField] private bool debugMode = true; 

    private Coroutine searchCoroutine;
    private Coroutine timerCoroutine;
    private bool isSearching = false;
    private int elapsedSeconds = 0;

    private void Start()
    {
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelSearch);
            cancelButton.gameObject.SetActive(false); 
        }
    }

    public void StartSearch()
    {
        if (isSearching) return;

        isSearching = true;
        elapsedSeconds = 0;

        if (cancelButton != null)
            cancelButton.gameObject.SetActive(true);

        if (iconManager != null)
            iconManager.UpdateSearchTimer(0);

        StopAllCoroutines();

        searchCoroutine = StartCoroutine(SearchRoutine());
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (isSearching)
        {
            yield return new WaitForSeconds(1f); 

            elapsedSeconds++;

            if (iconManager != null)
            {
                iconManager.UpdateSearchTimer(elapsedSeconds);
            }
        }
    }

    private IEnumerator SearchRoutine()
    {
        while (isSearching)
        {
            if (elapsedSeconds > 0 && elapsedSeconds % 2 == 0)
            {
                if (debugMode && Random.value < 0.2f)
                {
                    OnMatchFound();
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.5f); 
        }
    }

    private void OnMatchFound()
    {
        if (!isSearching) return;
        isSearching = false;

        StopAllCoroutines();

        if (cancelButton != null)
            cancelButton.gameObject.SetActive(false);

        if (iconManager != null)
        {
            StartCoroutine(ShowMatchFoundMessage());
        }
        else
        {
            StartCoroutine(TransitionToGameWithDelay(0.5f));
        }
    }

    private IEnumerator ShowMatchFoundMessage()
    {
        string originalText = iconManager.searchTimerText.text;
        Color originalColor = iconManager.searchTimerText.color;

        iconManager.searchTimerText.text = "MATCH FOUND!";
        iconManager.searchTimerText.color = Color.green;

        yield return new WaitForSeconds(1f);

        iconManager.searchTimerText.text = originalText;
        iconManager.searchTimerText.color = originalColor;

        StartCoroutine(TransitionToGameWithDelay(0.5f));
    }

    private IEnumerator TransitionToGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (MenuManager.Instance != null)
            MenuManager.Instance.OnMatchFound();
    }

    public void CancelSearch()
    {
        if (!isSearching) return;
        isSearching = false;

        StopAllCoroutines();

        if (cancelButton != null)
            cancelButton.gameObject.SetActive(false);

        if (iconManager != null)
            iconManager.HideSearchIcon();

        if (MenuManager.Instance != null)
            MenuManager.Instance.ClosePopup();
    }
}