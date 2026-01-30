using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Gameplay
{
    public class GameState
    {
        public int TurnIndex;
        public Dictionary<int, Pieces.PieceState> Pieces = new();

        public static GameState Create(List<PieceState> states) => new()
        {
            TurnIndex = 0,
            Pieces = states.ToDictionary(x => x.PieceId.Value, y => y),
        };

        public GameState Clone()
        {
            var copy = new GameState
            {
                TurnIndex = TurnIndex
            };

            foreach (var kv in Pieces)
            {
                var p = kv.Value;
                copy.Pieces[kv.Key] = new Pieces.PieceState
                {
                    PieceId = p.PieceId,
                    OwnerPlayerId = p.OwnerPlayerId,
                    CellId = p.CellId,
                    Mask = p.Mask,
                    Hp = p.Hp
                };
            }
            return copy;
        }
    }
}
