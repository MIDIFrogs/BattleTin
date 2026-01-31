using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode
{
    internal class MatchmakingPanel : MonoBehaviour
    {
        public Button startMatchButton;
        public Button cancelMatchButton;
        public Image searchingSpinner;

        private bool isMatchmaking = false;

        private void Awake()
        {
            //MatchmakingManager.Instance.CheckConnection().ContinueWith(x => startMatchButton.enabled = x).Forget();
        }

        private void Update()
        {
            searchingSpinner.fillAmount = Mathf.Clamp01(searchingSpinner.fillAmount + Time.deltaTime);
            if (searchingSpinner.fillAmount == 1)
                searchingSpinner.fillAmount = 0;
        }

        public void OnMatchStartClicked()
        {
            MatchmakingManager.Instance.QuickMatch();
            isMatchmaking = true;
            UpdateMatchmakingUI();
        }

        public void OnCancelClicked()
        {
            MatchmakingManager.Instance.CancelMatchmakingAsync().Forget();
            isMatchmaking = false;
            UpdateMatchmakingUI();
        }

        private void UpdateMatchmakingUI()
        {
            startMatchButton.gameObject.SetActive(!isMatchmaking);
            cancelMatchButton.gameObject.SetActive(isMatchmaking);
            searchingSpinner.gameObject.SetActive(isMatchmaking);
        }
    }
}
