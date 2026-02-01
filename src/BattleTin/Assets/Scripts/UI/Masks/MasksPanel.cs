using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Masks
{
    internal class MasksPanel : MonoBehaviour
    {
        [SerializeField] private TurnControllerBase turnController;
        [SerializeField] private HexSelectionManager hexManager;
        [SerializeField] private MaskSelectionUI selectionUI;

        private void Awake()
        {
            turnController.TurnFinished += s => UpdateView(s.Inventories[MatchmakingManager.Instance.LocalTeamId]);
        }

        public void UpdateView(MaskInventory inventory)
        {
            // Clear previous mask visuals
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var p in inventory.Counts)
            {
                var card = Instantiate(selectionUI, transform);
                card.turnController = turnController;
                card.hexManager = hexManager;
                card.UpdateMaskView(p.Key, p.Value);
            }
        }
    }
}
