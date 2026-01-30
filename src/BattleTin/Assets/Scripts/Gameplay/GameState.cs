using System.Collections.Generic;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Gameplay
{
    public class GameState
    {
        public int TurnIndex;
        public Dictionary<int, Pieces.PieceState> Pieces = new();

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
                    Cell = p.Cell,
                    Mask = p.Mask,
                    Hp = p.Hp
                };
            }
            return copy;
        }
    }
}
