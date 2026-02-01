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
        public GameObject playerContext;

        private void Awake()
        {
            turnController.GameStateUpdated += OnGameUpdated;
        }

        private void OnGameUpdated(Gameplay.GameState state)
        {
            if (state.GameOver)
            {
                panel.SetActive(true);
                verdictText.text = state.WinnerTeamId == playerContext.GetComponent<IPlayerContext>().LocalTeamId ? "You won!" : "You lost";
            }
        }
    }
}
