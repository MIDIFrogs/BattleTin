using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MIDIFrogs.BattleTin.Field
{
    public class Hex : MonoBehaviour, IPointerClickHandler
    {
        public Vector2Int gridCoordinates;
        
        public int CellId;
        public List<Hex> Neighbors;
        public List<Hex> DiagonalNeighbors;

        public void OnPointerClick(PointerEventData eventData)
        {
            HexSelectionManager.Instance.OnHexClicked(this);
        }
    }
}