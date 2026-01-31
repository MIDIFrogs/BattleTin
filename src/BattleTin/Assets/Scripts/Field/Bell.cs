using MIDIFrogs.BattleTin.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MIDIFrogs.BattleTin.Field
{
    [RequireComponent(typeof(TurnController))]
    [RequireComponent(typeof(Button3DHighlight))]
    public class Bell : MonoBehaviour, IPointerClickHandler
    {
        private TurnController turnController;
        private Button3DHighlight highlight;

        private void Awake()
        {
            turnController = GetComponent<TurnController>();
            highlight = GetComponent<Button3DHighlight>();

            turnController.TurnStarted += s => highlight.Select();
            turnController.TurnFinished += s => highlight.Deselect();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            turnController.ConfirmTurn();
        }
    }
}
