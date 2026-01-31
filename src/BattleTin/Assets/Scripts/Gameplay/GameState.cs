using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay
{
    public class GameState
    {
        public int TurnIndex;
        public Dictionary<int, PieceState> Pieces = new();
        public Dictionary<int, MaskInventory> Inventories = new();
        public readonly BoardGraph Board;

        public bool GameOver;
        public int WinnerTeamId;

        public static GameState Create(List<PieceState> states, List<MaskInventory> inventories, BoardGraph board) => new(board)
        {
            TurnIndex = 0,
            Pieces = states.ToDictionary(x => x.PieceId.Value, y => y),
            Inventories = inventories.ToDictionary(x => x.TeamId, y => y),
        };

        private GameState(BoardGraph board) => Board = board;

        public GameState Clone()
        {
            var copy = new GameState(Board)
            {
                TurnIndex = TurnIndex,
                Inventories = Inventories.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            };

            foreach (var kv in Pieces)
            {
                var p = kv.Value;
                copy.Pieces[kv.Key] = new PieceState
                {
                    PieceId = p.PieceId,
                    TeamId = p.TeamId,
                    CellId = p.CellId,
                    Mask = p.Mask,
                    Health = p.Health
                };
            }
            return copy;
        }

        public bool IsCellFreeFor(PieceState piece, CellId cell)
        {
            var targetPiece = Pieces.Values.FirstOrDefault(x => x.CellId == cell);
            return targetPiece == null || targetPiece.TeamId != piece.TeamId;
        }
    }
}
