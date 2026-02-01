using System;
using System.Collections.Generic;
using System.Text;
using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Masks
{
    internal class MaskPanelShared : MonoBehaviour
    {
        [Header("Control")]
        [SerializeField] private TurnControllerBase turnController;
        [SerializeField] private HexSelectionManager hexManager;

        [Header("UI")]
        [SerializeField] private Image background;
        [SerializeField] private Button[] buttons;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_Text confirmText;

        [Header("Resources")]
        [SerializeField] private MaskType[] maskTypes;
        [SerializeField] private Sprite[] textures;

        public MaskType SelectedMask { get; private set; }

        private void Awake()
        {
            OnButtonClick(0);
        }

        public void OnButtonClick(int buttonId)
        {
            SelectedMask = maskTypes[buttonId];
            background.sprite = textures[buttonId];
            buttons[buttonId].Select();

            if (SelectedMask == MaskType.None || SelectedMask == MaskType.Captain || SelectedMask == MaskType.Barricade)
            {
                confirmButton.enabled = false;
                confirmText.text = "Нельзя надеть";
            }
            else
            {
                confirmButton.enabled = true;
                confirmText.text = "Надеть";
            }
        }

        public void OnConfirmMask()
        {
            if (hexManager.SelectedPiece == null || SelectedMask == MaskType.None)
                return;

            turnController.SetLocalOrder(
                MoveOrder.EquipMask(
                    turn: turnController.TurnIndex,
                    teamId: MatchmakingManager.Instance.LocalTeamId,
                    pieceId: hexManager.SelectedPiece.PieceId,
                    mask: SelectedMask
                )
            );

            gameObject.SetActive(false);
        }
    }
}
