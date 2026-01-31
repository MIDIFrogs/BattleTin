using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    public sealed class ExclusiveDirectStepRule : IMovementRule
    {
        private readonly DirectStepRule direct;
        private readonly DiagonalStepRule diagonal;

        private readonly int everyNTurns;

        public ExclusiveDirectStepRule(int maxDistance, int everyNTurns = 1)
        {
            direct = new(maxDistance);
            diagonal = new(maxDistance - 1);
            this.everyNTurns = everyNTurns;
        }

        public bool CanMove(PieceState piece, GameState state, CellId target)
            => GetAvailableCells(piece, state).Contains(target);

        public IEnumerable<CellId> GetAvailableCells(PieceState piece, GameState state)
        {
            // Check if we need to wait for more turns
            if ((state.TurnIndex - piece.LastActionTurn) < everyNTurns - 1)
            {
                Debug.Log($"Piece ID {piece.CellId} must wait for {everyNTurns} turns. Exiting.");
                return Enumerable.Empty<CellId>();
            }

            var directCells = direct.GetAvailableCells(piece, state).ToHashSet();
            var diagonalCells = diagonal.GetAvailableCells(piece, state).ToHashSet();

            return directCells.Except(diagonalCells);
        }
    }
}
