using System;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay.Orders
{
    [Serializable]
    public struct MoveOrder : IEquatable<MoveOrder>
    {
        public int TurnIndex;
        public int TeamId;
        public int PieceId;
        public OrderType Type;

        public int TargetCellId;
        public MaskType Mask;

        public static MoveOrder Move(int turn, int teamId, int pieceId, int cellId) => new()
        {
            TurnIndex = turn,
            TeamId = teamId,
            PieceId = pieceId,
            Type = OrderType.Move,
            TargetCellId = cellId
        };

        public static MoveOrder EquipMask(int turn, int teamId, int pieceId, MaskType mask) => new()
        {
            TurnIndex = turn,
            TeamId = teamId,
            PieceId = pieceId,
            Type = OrderType.EquipMask,
            Mask = mask,
        };

        public bool Equals(MoveOrder other) =>
            TurnIndex == other.TurnIndex &&
            TeamId == other.TeamId &&
            PieceId == other.PieceId &&
            Type == other.Type &&
            TargetCellId == other.TargetCellId &&
            Mask == other.Mask;

        public override bool Equals(object obj) => obj is MoveOrder order && Equals(order);

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(TurnIndex, TeamId, PieceId, Type, TargetCellId, Mask);
        }

        public override string ToString()
        {
            return $"MoveOrder: [TurnIndex: {TurnIndex}, TeamId: {TeamId}, PieceId: {PieceId}, Type: {Type}, TargetCellId: {TargetCellId}, Mask: {Mask}]";
        }
    }
}
