using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MIDIFrogs.BattleTin.Field
{
    [RequireComponent(typeof(OfflineTurnController))]
    public class OfflineBell : MonoBehaviour, IPointerClickHandler
    {
        private OfflineTurnController offlineTurnController;

        private void Awake()
        {
            offlineTurnController = GetComponent<OfflineTurnController>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            offlineTurnController.ConfirmTurn();
        }
    }
}
