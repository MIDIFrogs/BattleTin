using System;
using MIDIFrogs.BattleTin.Gameplay.Board;
using UnityEditor;

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
        private int teamId;
        private CellId cell;
        private MaskType mask;
        private int hp;
        private int lastActionTurn;

        public PieceId PieceId { get => id; set => id = value; }
        public int TeamId { get => teamId; set => teamId = value; }
        public MaskType Mask { get => mask; set => mask = value; }
        public int Hp { get => hp; set => hp = value; }
        public CellId CellId { get => cell; set => cell = value; }
        public int LastActionTurn { get => lastActionTurn; set => lastActionTurn = value; }
        public bool IsAlive => Hp > 0;

        public bool IsMine(int myTeam, PieceState unit)
        {
            return unit.TeamId == myTeam;
        }
    }
}
