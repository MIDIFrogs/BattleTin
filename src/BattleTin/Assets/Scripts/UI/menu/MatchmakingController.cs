using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;

public class MatchmakingController : MonoBehaviour
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
        MatchmakingManager.Instance.QuickMatch();

        isSearching = true;
        elapsedSeconds = 0;

        if (cancelButton != null)
            cancelButton.gameObject.SetActive(true);

        if (iconManager != null)
            iconManager.UpdateSearchTimer(0);

        StopAllCoroutines();

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