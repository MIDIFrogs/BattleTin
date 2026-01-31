using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public interface ITurnController
    {
        int TurnIndex { get; }

        TurnPhase Phase { get; }

        void InitializeGameState(GameState state);

        void SetLocalOrder(MoveOrder order);
    }
}
