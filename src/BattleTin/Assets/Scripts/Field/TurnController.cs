using System;
using MIDIFrogs.BattleTin.Core;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Netcode;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    [RequireComponent(typeof(TurnSyncManager))]
    [RequireComponent(typeof(TurnAnimator))]
    public class TurnController : MonoBehaviour
    {
        private GameState gameState;
        private TurnAnimator turnAnimator;
        private TurnSyncManager turnSyncManager;

        public MoveOrder? LocalOrder { get; private set; }
        public TurnPhase Phase { get; private set; }
        public MoveOrder? RemoteOrder { get; private set; }
        public int TurnIndex { get; private set; }
        public GameState GameState => gameState;

        public event Action<MoveOrder> OrderUpdated = delegate { };
        public event Action<GameState> TurnStarted = delegate { };
        public event Action<GameState> TurnFinished = delegate { };

        public void InitializeGameState(GameState initialState)
        {
            gameState = initialState;
            Debug.Log($"Game state initialized: {gameState}");
        }

        public void ConfirmTurn()
        {
            if (Phase != TurnPhase.Planning)
                return;

            // TODO: lock the UI
            Debug.Log("Confirming turn...");

            if (!LocalOrder.HasValue)
            {
                LocalOrder = CreatePassOrder();
                Debug.Log("Local order was not set. Created a Pass order.");
            }

            Phase = TurnPhase.WaitingForRemote;
            TurnStarted(gameState);
            Debug.Log($"Turn started: Index {TurnIndex}, State: {gameState}");

            turnSyncManager.SendOrder(LocalOrder.Value);
            if (!LocalOrder.HasValue || !RemoteOrder.HasValue)
                return;

            Phase = TurnPhase.Resolving;
            ResolveTurn();
        }

        private MoveOrder? CreatePassOrder() => new()
        {
            Mask = Gameplay.Pieces.MaskType.None,
            PieceId = 0,
            Type = OrderType.Pass,
            TeamId = MatchmakingManager.Instance.LocalTeamId,
            TargetCellId = 0,
            TurnIndex = TurnIndex,
        };

        public void SetLocalOrder(MoveOrder order)
        {
            if (Phase != TurnPhase.Planning)
                return;

            LocalOrder = order;
            OrderUpdated(LocalOrder ?? CreatePassOrder().Value);
            Debug.Log($"Local order set: {LocalOrder}");
        }

        private void Awake()
        {
            turnSyncManager = GetComponent<TurnSyncManager>();
            turnSyncManager.OnRemoteConfirmed += TryResolve;
            turnAnimator = GetComponent<TurnAnimator>();
        }

        void FinishTurn()
        {
            Debug.Log($"Finishing turn: Index {TurnIndex}");

            LocalOrder = null;
            RemoteOrder = null;
            OrderUpdated(LocalOrder ?? CreatePassOrder().Value);

            TurnIndex++;
            Phase = TurnPhase.Planning;

            TurnFinished(gameState);
            Debug.Log($"Turn finished. Next turn index: {TurnIndex}, State: {gameState}");
        }

        private void ResolveTurn()
        {
            Debug.Log("Resolving turn...");

            var oldState = gameState;
            var newState = TurnResolver.Resolve(
                gameState,
                LocalOrder.Value,
                RemoteOrder.Value
            );

            gameState = newState;
            turnAnimator.AnimateDiff(oldState, newState);
            FinishTurn();
        }

        private void TryResolve(MoveOrder obj)
        {
            Debug.Log($"Received remote order: {obj.Type}");

            RemoteOrder = obj;
            if (!LocalOrder.HasValue || !RemoteOrder.HasValue)
                return;

            Phase = TurnPhase.Resolving;
            Debug.Log($"Turn being resolved with Local order: {LocalOrder}, Remote order: {RemoteOrder}");
            ResolveTurn();
        }
    }
}
