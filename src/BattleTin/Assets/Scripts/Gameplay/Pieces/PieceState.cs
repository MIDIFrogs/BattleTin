using System;
using MIDIFrogs.BattleTin.Gameplay.Board;

namespace MIDIFrogs.BattleTin.Gameplay.Pieces
{
    [Serializable]
    public struct PieceId
    {
        public int Value;
    }

    [Serializable]
    public class PieceState
    {
        private PieceId id;
        private int ownerPlayerId;
        private CellId cell;
        private MaskType mask;
        private int hp;

        public PieceId PieceId { get => id; set => id = value; }
        public int OwnerPlayerId { get => ownerPlayerId; set => ownerPlayerId = value; }
        public MaskType Mask { get => mask; set => mask = value; }
        public int Hp { get => hp; set => hp = value; }
        internal CellId Cell { get => cell; set => cell = value; }
    }
}
