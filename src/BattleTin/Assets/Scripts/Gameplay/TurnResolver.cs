using System;
using System.Collections.Generic;
using System.Text;

namespace MIDIFrogs.BattleTin.Gameplay.Assets.Scripts.Gameplay
{
    public static class TurnResolver
    {

        public static GameState Resolve(
            GameState state,
            Orders.MoveOrder a,
            Orders.MoveOrder b
        )
        {
            var next = state.Clone();
            next.TurnIndex++;

            // TODO:
            // 1. Apply masks
            // 2. Apply movement
            // 3. Resolve battles

            return next;
        }
    }
}
