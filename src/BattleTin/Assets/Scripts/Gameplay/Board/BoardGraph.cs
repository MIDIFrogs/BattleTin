using System;
using System.Collections.Generic;

namespace MIDIFrogs.BattleTin.Gameplay.Board
{
    [Serializable]
    public readonly struct CellId
    {
        public readonly int Value;

        public CellId(int value)
        {
            Value = value;
        }

        public override int GetHashCode() => Value;
        public override bool Equals(object obj)
            => obj is CellId other && other.Value == Value;
    }

    public class BoardGraph
    {
        private Dictionary<CellId, CellId[]> directConnections = new();
        private Dictionary<CellId, CellId[]> diagonalConnections = new();
    }
}
