using System;
using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using MIDIFrogs.FutureInThePast.UI.Dialogs;
using UnityEngine;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Tutorial
{
    public sealed class TutorialTurnController : TurnControllerBase
    {
        [SerializeField] private DialogPlayer dialogPlayer;
        [SerializeField] private Dialog[] dialog;

        public override int TurnIndex { get; protected set; }
        public override GameState GameState { get; protected set; }

        private MoveOrder? localOrder;
        private IBotPlayer bot;

        private const float FAKE_TURN_DURATION = 999f;

        private void Awake()
        {
            bot = new ScriptedBotPlayer(dialogPlayer, dialog);
        }

        public override void InitializeGameState(GameState state)
        {
            GameState = state;
            TurnIndex = state.TurnIndex;
            StartTurn();
        }

        public override void SetLocalOrder(MoveOrder order)
        {
            localOrder = order;
            OnOrderUpdated(order);
        }

        public override void ConfirmTurn()
        {
            if (!localOrder.HasValue)
                localOrder = CreatePass(0);

            OnOrderSubmitted(localOrder.Value);

            var botOrder = bot.DecideOrder(GameState, TurnIndex, 1);

            Resolve(localOrder.Value, botOrder);
        }

        private void StartTurn()
        {
            localOrder = null;
            OnTurnTimerStarted(FAKE_TURN_DURATION);
            OnTurnStarted(GameState);
        }

        private void Resolve(MoveOrder a, MoveOrder b)
        {
            GameState = TurnResolver.Resolve(GameState, a, b);
            OnGameStateUpdated(GameState);
            OnTurnFinished(GameState);

            if (GameState.GameOver)
                return;

            TurnIndex++;
            StartTurn();
        }

        private MoveOrder CreatePass(int team)
        {
            return new MoveOrder
            {
                TurnIndex = TurnIndex,
                TeamId = team,
                Type = OrderType.Pass
            };
        }
    }

}
