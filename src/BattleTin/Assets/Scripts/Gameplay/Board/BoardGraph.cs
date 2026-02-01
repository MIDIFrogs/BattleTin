using System;
using System.Collections.Generic;
using System.Linq;

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

        public static implicit operator CellId(int value) => new(value);

        public static bool operator ==(CellId left, CellId right) => left.Equals(right);

        public static bool operator !=(CellId left, CellId right) => !(left == right);
    }

    public class BoardGraph
    {
        private readonly Node[] _nodes;

        public BoardGraph(int size)
        {
            _nodes = new Node[size];
        }

        public void AddConnections(CellId target, IEnumerable<CellId> directConnections, IEnumerable<CellId> diagonalConnections)
        {
            _nodes[target.Value] = new Node()
            {
                Id = target,
                DirectConnections = directConnections.ToHashSet(),
                DiagonalConnections = diagonalConnections.ToHashSet(),
            };
        }

        public IEnumerable<CellId> AllCells => _nodes.Select(x => x.Id);

        public bool IsDirectConnected(CellId from, CellId to) => _nodes[from.Value].DirectConnections.Contains(to);

        public bool IsDiagonallyConnected(CellId from, CellId to) => _nodes[from.Value].DiagonalConnections.Contains(to);

        public IEnumerable<CellId> GetDirectNeighbors(CellId cell) => _nodes[cell.Value].DirectConnections;

        public IEnumerable<CellId> GetDiagonalNeighbors(CellId cell) => _nodes[cell.Value].DiagonalConnections;

        private struct Node
        {
            public CellId Id;

            public HashSet<CellId> DirectConnections;
            public HashSet<CellId> DiagonalConnections;
        }
    }
}
