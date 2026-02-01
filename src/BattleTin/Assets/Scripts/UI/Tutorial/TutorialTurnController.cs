using System;
using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using MIDIFrogs.FutureInThePast.UI.Dialogs;
using UnityEngine;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Tutorial
{
    [RequireComponent(typeof(TurnAnimator))]
    public sealed class TutorialTurnController : TurnControllerBase
    {
        [SerializeField] private DialogPlayer dialogPlayer;
        [SerializeField] private Dialog[] dialog;

        private TurnAnimator animator;

        public override int TurnIndex { get; protected set; }
        public override GameState GameState { get; protected set; }

        private MoveOrder? localOrder;
        private IBotPlayer bot;

        private const float FAKE_TURN_DURATION = 999f;

        private void Awake()
        {
            bot = new ScriptedBotPlayer(dialogPlayer, dialog);
            animator = GetComponent<TurnAnimator>();
            SetupAnimator(animator);
            Debug.Log("TutorialTurnController Awake: Bot and Animator set up.");
        }

        public override void InitializeGameState(GameState state)
        {
            GameState = state;
            TurnIndex = state.TurnIndex;
            StartTurn();
            Debug.Log($"GameState initialized: TurnIndex set to {TurnIndex}.");
        }

        public override void SetLocalOrder(MoveOrder order)
        {
            localOrder = order;
            Debug.Log($"Local order set: {order}");
            OnOrderUpdated(order);
        }

        public override void ConfirmTurn()
        {
            if (!localOrder.HasValue)
            {
                Debug.Log("No local order set, creating a pass.");
                localOrder = CreatePass(0);
            }
            else
            {
                Debug.Log($"Local order confirmed: {localOrder.Value}");
            }

            OnOrderSubmitted(localOrder.Value);

            var botNewState = GameState.Clone();
            var botOrder = bot.DecideOrder(botNewState, localOrder.Value, TurnIndex, 1);
            Debug.Log($"Bot order decided: {botOrder}");
            animator.AnimateDiff(GameState, botNewState);
            GameState = botNewState;
            OnGameStateUpdated(botNewState);

            Resolve(localOrder.Value, botOrder);
        }

        private void StartTurn()
        {
            localOrder = null;
            Debug.Log("Starting turn, local order reset.");
            OnTurnTimerStarted(FAKE_TURN_DURATION);
            OnTurnStarted(GameState);
            Debug.Log("Turn timer started.");
        }

        private void Resolve(MoveOrder a, MoveOrder b)
        {
            var oldState = GameState;
            GameState = TurnResolver.Resolve(GameState, a, b);
            animator.AnimateDiff(oldState, GameState);
            animator.PlayBattle(a, b, oldState, GameState);

            OnGameStateUpdated(GameState);
            OnTurnFinished(GameState);
            Debug.Log("Turn resolved.");

            if (GameState.GameOver)
            {
                Debug.Log("Game over!");
                return;
            }

            TurnIndex++;
            Debug.Log($"Turn Index incremented to {TurnIndex}.");
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
