using System;
using MIDIFrogs.BattleTin.Gameplay.Board;

namespace MIDIFrogs.BattleTin.Gameplay.Pieces
{
    [Serializable]
    public struct PieceId
    {
        public int Value;

        public PieceId(int value)
        {
            Value = value;
        }

        public static implicit operator PieceId(int pieceId) => new PieceId(pieceId);
    }

    [Serializable]
    public class PieceState
    {
        private PieceId id;
        private ulong ownerPlayerId;
        private CellId cell;
        private MaskType mask;
        private int hp;

        public PieceId PieceId { get => id; set => id = value; }
        public ulong OwnerPlayerId { get => ownerPlayerId; set => ownerPlayerId = value; }
        public MaskType Mask { get => mask; set => mask = value; }
        public int Hp { get => hp; set => hp = value; }
        public CellId CellId { get => cell; set => cell = value; }
    }
}
