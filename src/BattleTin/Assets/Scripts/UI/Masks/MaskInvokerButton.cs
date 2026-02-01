using System;
using System.Collections.Generic;
using System.Text;
using MIDIFrogs.BattleTin.Field;
using UnityEngine;
using UnityEngine.UI;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Masks
{
    class MaskInvokerButton : MonoBehaviour
    {
        [SerializeField] private HexSelectionManager hexManager;
        [SerializeField] private Button button;
        [SerializeField] private GameObject panel;

        private void Awake()
        {
            hexManager.SelectionUpdated += OnSelectionUpdated;
        }

        private void OnSelectionUpdated(PieceView obj)
        {
            if (obj == null)
            {
                button.interactable = false;
                panel.SetActive(false);
            }
            else
            {
                button.interactable = true;
            }
        }

        public void OnClick()
        {
            panel.SetActive(true);
        }
    }
}
