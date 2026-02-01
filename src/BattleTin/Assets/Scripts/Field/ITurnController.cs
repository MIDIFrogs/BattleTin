using System;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;

namespace MIDIFrogs.BattleTin.Field
{
    public interface ITurnController
    {
        int TurnIndex { get; }
        GameState GameState { get; }

        event Action<GameState> TurnStarted;
        event Action<GameState> TurnFinished;
        event Action<float> TurnTimerStarted;
        event Action<GameState> GameStateUpdated;
        event Action<MoveOrder> OrderUpdated;
        event Action<MoveOrder> OrderSubmitted;

        void InitializeGameState(GameState state);

        void SetLocalOrder(MoveOrder order);

        void ConfirmTurn();
    }
}
