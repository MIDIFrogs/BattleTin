using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    public sealed class DirectStepRule : IMovementRule
    {
        private readonly int maxSteps;
        private readonly int everyNTurns;

        public DirectStepRule(int maxSteps, int everyNTurns = 1)
        {
            this.maxSteps = maxSteps;
            this.everyNTurns = everyNTurns;
        }

        public IEnumerable<CellId> GetAvailableCells(PieceState piece, GameState state)
        {
            Debug.Log($"Calculating available cells for Piece ID: {piece.CellId}");

            // Check if we need to wait for more turns
            if ((state.TurnIndex - piece.LastActionTurn) < everyNTurns - 1)
            {
                Debug.Log($"Piece ID {piece.CellId} must wait for {everyNTurns} turns. Exiting.");
                yield break;
            }

            var visited = new HashSet<CellId> { piece.CellId };
            var frontier = new Queue<(CellId, int)>();
            frontier.Enqueue((piece.CellId, 0));

            Debug.Log($"Starting exploration from Cell ID: {piece.CellId}");

            while (frontier.Count > 0)
            {
                var (cell, dist) = frontier.Dequeue();

                Debug.Log($"Dequeueing Cell ID: {cell} at distance: {dist}");

                if (dist == maxSteps)
                {
                    Debug.Log($"Max steps reached at Cell ID: {cell}. Continuing to next cell.");
                    continue;
                }

                foreach (var next in state.Board.GetDirectNeighbors(cell))
                {
                    if (!visited.Add(next))
                    {
                        Debug.Log($"Cell ID: {next} already visited. Skipping.");
                        continue;
                    }

                    if (!state.IsCellFreeFor(piece, next))
                    {
                        Debug.Log($"Cell ID: {next} is not free for Piece ID {piece.CellId}. Skipping.");
                        continue;
                    }

                    Debug.Log($"Yielding available Cell ID: {next}");
                    yield return next;
                    frontier.Enqueue((next, dist + 1));
                    Debug.Log($"Enqueued Cell ID: {next} with distance: {dist + 1}");
                }
            }

            Debug.Log($"Completed exploration for Piece ID: {piece.CellId}. Visited Cells: {string.Join(", ", visited)}");
        }


        public bool CanMove(PieceState piece, GameState state, CellId target)
            => GetAvailableCells(piece, state).Contains(target);
    }
}
