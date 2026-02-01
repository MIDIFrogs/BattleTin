using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay;
using UnityEngine;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Masks
{
    internal class MasksPanel : MonoBehaviour
    {
        [SerializeField] private TurnControllerBase turnController;
        [SerializeField] private HexSelectionManager hexManager;
        [SerializeField] private MaskSelectionUI selectionUI;
        [SerializeField] private GameObject playerContext;

        private int teamId;

        private void Awake()
        {
            teamId = playerContext.GetComponent<IPlayerContext>().LocalTeamId;
            turnController.TurnFinished += s => UpdateView(s.Inventories[teamId]);
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
                card.teamId = teamId;
                card.UpdateMaskView(p.Key, p.Value);
            }
        }
    }
}
