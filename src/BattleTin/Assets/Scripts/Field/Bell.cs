using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MIDIFrogs.BattleTin.Field
{
    [RequireComponent(typeof(TurnController))]
    public class Bell : MonoBehaviour, IPointerClickHandler
    {
        private TurnController turnController;

        private void Awake()
        {
            turnController = GetComponent<TurnController>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            turnController.ConfirmTurn();
        }
    }
}
