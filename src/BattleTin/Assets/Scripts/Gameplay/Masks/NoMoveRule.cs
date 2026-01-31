using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    internal class NoMoveRule : IMovementRule
    {
        public bool CanMove(PieceState piece, GameState state, CellId target) => false;

        public IEnumerable<CellId> GetAvailableCells(PieceState piece, GameState state) => Enumerable.Empty<CellId>();
    }
}
