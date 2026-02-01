using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Masks;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    public sealed class JumpOverRule : IMovementRule
    {
        public IEnumerable<CellId> GetAvailableCells(
            PieceState piece,
            GameState state
        )
        {
            foreach (var c in state.Board.GetDirectNeighbors(piece.CellId))
            {
                foreach (var n in state.Board.GetDirectNeighbors(c))
                {
                    if (!state.IsCellFreeFor(piece, c)) continue;
                    yield return n;
                }
            }
        }

        public bool CanMove(PieceState piece, GameState state, CellId target)
            => GetAvailableCells(piece, state).Contains(target);
    }
}
