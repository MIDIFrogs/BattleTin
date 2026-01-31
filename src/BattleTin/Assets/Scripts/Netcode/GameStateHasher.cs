using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIDIFrogs.BattleTin.Gameplay;

namespace MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode
{
    public static class GameStateHasher
    {
        public static int Compute(GameState state)
        {
            unchecked
            {
                int hash = 17;

                foreach (var piece in state.Pieces.Values.OrderBy(p => p.PieceId.Value))
                {
                    hash = hash * 31 + piece.PieceId.Value;
                    hash = hash * 31 + piece.TeamId;
                    hash = hash * 31 + piece.CellId.Value;
                    hash = hash * 31 + piece.Health;
                    hash = hash * 31 + (int)piece.Mask;
                }

                foreach (var inventory in state.Inventories.Values.OrderBy(i => i.TeamId))
                {
                    foreach (var count in inventory.Counts.OrderBy(c => c.Key))
                        hash = hash * 31 + count.Value;
                }

                return hash;
            }
        }
    }
}
