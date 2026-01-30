using System.Collections.Generic;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field.Assets.Scripts.Field
{
    public class CellView : MonoBehaviour
    {
        public int CellId;
        public List<CellView> Neighbors;
        public List<CellView> DiagonalNeighbors;
    }
}
