using MIDIFrogs.BattleTin.Field;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI
{
    internal class NextTurnButton : MonoBehaviour
    {
        private TurnControllerBase turnController;
        private Button button;
        

        private void Awake()
        {
            turnController = GetComponent<TurnControllerBase>();
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClicked);

            turnController.OrderSubmitted += s => button.enabled = false;
            turnController.TurnFinished += s =>
            {
                button.enabled = true;
            };
        }

        public void OnClicked() => turnController.ConfirmTurn();
    }
}
