using System.Linq;
using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MIDIFrogs.BattleTin.UI
{
    public class MaskSelectionUI : MonoBehaviour
    {
        public TurnController turnController;
        public HexSelectionManager hexManager;

        [Header("Visual parameters")]
        [SerializeField] private Image maskIcon;
        [SerializeField] private TMP_Text maskName;
        [SerializeField] private TMP_Text maskCount;

        [SerializeField] private MaskVisualizationOptions maskVisualizationOptions;

        private MaskType maskType;

        public void OnMaskClicked()
        {
            if (hexManager.SelectedPiece == null || maskType == MaskType.None)
                return;

            turnController.SetLocalOrder(
                MoveOrder.EquipMask(
                    turn: turnController.TurnIndex,
                    teamId: MatchmakingManager.Instance.LocalTeamId,
                    pieceId: hexManager.SelectedPiece.PieceId,
                    mask: maskType
                )
            );
        }

        public void UpdateMaskView(MaskType type, int count)
        {
            maskType = type;

            var visual = maskVisualizationOptions.GetVisualization(type);
            if (visual == null)
            {
                Debug.LogWarning($"Cannot visualize mask of type {type}");
                maskIcon.sprite = null;
                maskName.text = type.ToString();
            }
            else
            {
                maskIcon.sprite = visual.MaskSprite;
                maskName.text = visual.MaskName;
            }

            maskCount.text = count.ToString();
        }
    }

}
