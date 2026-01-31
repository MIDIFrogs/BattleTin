using MIDIFrogs.BattleTin.Field;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI
{
    [RequireComponent(typeof(TurnController))]
    internal class NextTurnButton : MonoBehaviour
    {
        private TurnController turnController;
        private Button button;
        
        [SerializeField] private TMP_Text turnIndicator;

        private void Awake()
        {
            turnController = GetComponent<TurnController>();
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClicked);

            turnController.OrderSubmitted += s => button.enabled = false;
            turnController.TurnFinished += s =>
            {
                button.enabled = true;
                turnIndicator.text = $"Turn #{turnController.TurnIndex}";
            };
        }

        public void OnClicked() => turnController.ConfirmTurn();
    }
}
