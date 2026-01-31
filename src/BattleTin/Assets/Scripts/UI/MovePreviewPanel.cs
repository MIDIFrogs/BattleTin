using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI
{
    internal class MovePreviewPanel : MonoBehaviour
    {
        [SerializeField] private Image iconFrom;
        [SerializeField] private Image iconTo;
        [SerializeField] private TMP_Text action;
        [SerializeField] private TMP_Text fromText;
        [SerializeField] private TMP_Text toText;

        [SerializeField] private TurnController turnController;
        [SerializeField] private MaskVisualizationOptions maskVisualizationOptions;

        private void Awake()
        {
            turnController.OrderUpdated += OnOrderUpdated;
        }

        private void OnOrderUpdated(MoveOrder order)
        {
            if (order.Type != OrderType.Pass)
            {
                var piece = turnController.GameState.Pieces[order.PieceId];
                var visualization = maskVisualizationOptions.GetVisualization(piece.Mask);
                iconFrom.sprite = visualization?.MaskSprite;
                fromText.text = visualization?.MaskName;
            }

            switch (order.Type)
            {
                case OrderType.Pass:
                    action.text = "Pass";
                    iconFrom.gameObject.SetActive(false);
                    iconTo.gameObject.SetActive(false);
                    fromText.gameObject.SetActive(false);
                    toText.gameObject.SetActive(false);
                    break;
                case OrderType.Move:
                    action.text = "Moving to";
                    iconFrom.gameObject.SetActive(true);
                    iconTo.gameObject.SetActive(true);
                    fromText.gameObject.SetActive(true);
                    toText.gameObject.SetActive(true);
                    toText.text = order.TargetCellId.ToString();
                    iconTo.sprite = null;
                    break;
                case OrderType.EquipMask:
                    action.text = "Equipping";
                    iconFrom.gameObject.SetActive(true);
                    iconTo.gameObject.SetActive(true);
                    fromText.gameObject.SetActive(true);
                    toText.gameObject.SetActive(true);
                    var toVisualization = maskVisualizationOptions.GetVisualization(order.Mask);
                    toText.text = toVisualization?.MaskName;
                    iconTo.sprite = toVisualization?.MaskSprite;
                    break;
            }
        }
    }
}
