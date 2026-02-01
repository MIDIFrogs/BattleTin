using System;
using System.Collections.Generic;
using System.Text;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    public sealed class KingMoveRule : IMovementRule
    {
        public IEnumerable<CellId> GetAvailableCells(
            PieceState piece,
            GameState state
        )
        {
            foreach (var c in state.Board.GetDirectNeighbors(piece.CellId))
            {
                if (!state.IsCellFreeFor(piece, c)) continue;
                yield return c;
            }

            foreach (var c in state.Board.GetDiagonalNeighbors(piece.CellId))
            {
                if (!state.IsCellFreeFor(piece, c)) continue;
                yield return c;
            }
        }

        public bool CanMove(PieceState piece, GameState state, CellId target)
            => (state.Board.IsDirectConnected(piece.CellId, target) || state.Board.IsDiagonallyConnected(piece.CellId, target)) && state.IsCellFreeFor(piece, target);
    }
}
