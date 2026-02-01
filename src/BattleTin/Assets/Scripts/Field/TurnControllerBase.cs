using System;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public abstract class TurnControllerBase : MonoBehaviour, ITurnController
    {
        public abstract int TurnIndex { get; protected set; }
        public abstract GameState GameState { get; protected set; }

        public event Action<GameState> TurnStarted;
        public event Action<GameState> TurnFinished;
        public event Action<float> TurnTimerStarted;
        public event Action<GameState> GameStateUpdated;
        public event Action<MoveOrder> OrderUpdated;
        public event Action<MoveOrder> OrderSubmitted;

        public abstract void ConfirmTurn();
        public abstract void InitializeGameState(GameState state);
        public abstract void SetLocalOrder(MoveOrder order);

        protected virtual void OnTurnStarted(GameState state)
        {
            TurnStarted?.Invoke(state);
        }

        protected virtual void OnTurnFinished(GameState state)
        {
            TurnFinished?.Invoke(state);
        }

        protected virtual void OnTurnTimerStarted(float duration)
        {
            TurnTimerStarted?.Invoke(duration);
        }

        protected virtual void OnGameStateUpdated(GameState state)
        {
            GameStateUpdated?.Invoke(state);
        }

        protected virtual void OnOrderUpdated(MoveOrder order)
        {
            OrderUpdated?.Invoke(order);
        }

        protected virtual void OnOrderSubmitted(MoveOrder order)
        {
            OrderSubmitted?.Invoke(order);
        }
    }
}
