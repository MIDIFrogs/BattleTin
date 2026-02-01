using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    public sealed class FreeTeleportRule : IMovementRule
    {
        public IEnumerable<CellId> GetAvailableCells(
            PieceState piece,
            GameState state
        )
        {
            foreach (var cell in state.Board.AllCells)
            {
                if (state.IsCellFreeFor(piece, cell))
                    yield return cell;
            }
        }

        public bool CanMove(PieceState piece, GameState state, CellId target)
            => state.IsCellFreeFor(piece, target);
    }

}
