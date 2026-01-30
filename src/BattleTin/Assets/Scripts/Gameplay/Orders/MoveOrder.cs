using System;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay.Orders
{
    [Serializable]
    public struct MoveOrder : IEquatable<MoveOrder>
    {
        public int TurnIndex;
        public ulong PlayerId;
        public int PieceId;
        public OrderType Type;

        public int TargetCellId;
        public MaskType Mask;

        public static MoveOrder Move(int turn, ulong playerId, int pieceId, int cellId) => new()
        {
            TurnIndex = turn,
            PlayerId = playerId,
            PieceId = pieceId,
            Type = OrderType.Move,
            TargetCellId = cellId
        };

        public bool Equals(MoveOrder other) =>
            TurnIndex == other.TurnIndex &&
            PlayerId == other.PlayerId &&
            PieceId == other.PieceId &&
            Type == other.Type &&
            TargetCellId == other.TargetCellId &&
            Mask == other.Mask;

        public override bool Equals(object obj) => obj is MoveOrder order && Equals(order);

        public override int GetHashCode()
        {
            return HashCode.Combine(TurnIndex, PlayerId, PieceId, Type, TargetCellId, Mask);
        }
    }
}
