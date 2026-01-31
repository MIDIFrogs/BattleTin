using System.Collections.Generic;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    public interface IMovementRule
    {
        IEnumerable<CellId> GetAvailableCells(
            PieceState piece,
            GameState state
        );

        bool CanMove(
            PieceState piece,
            GameState state,
            CellId target
        );
    }
}
