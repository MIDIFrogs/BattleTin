using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using TMPro;
using UnityEngine;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI
{
    internal class EndgamePanel : MonoBehaviour
    {
        public TMP_Text verdictText;
        public TurnControllerBase turnController;
        public GameObject panel;

        private void Awake()
        {
            turnController.GameStateUpdated += OnGameUpdated;
        }

        private void OnGameUpdated(Gameplay.GameState state)
        {
            if (state.GameOver)
            {
                panel.SetActive(true);
                verdictText.text = state.WinnerTeamId == MatchmakingManager.Instance.LocalTeamId ? "You won!" : "You lost";
            }
        }
    }
}
