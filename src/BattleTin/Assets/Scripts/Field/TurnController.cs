using System;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Netcode;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    [RequireComponent(typeof(TurnSyncManager))]
    [RequireComponent(typeof(TurnAnimator))]
    public class TurnController : MonoBehaviour
    {
        private GameState gameState;
        private TurnAnimator turnAnimator;
        private TurnSyncManager sync;

        private bool isDesynced;

        public MoveOrder? LocalOrder { get; private set; }
        public int TurnIndex { get; private set; }

        public event Action<GameState> TurnStarted = delegate { };
        public event Action<GameState> TurnFinished = delegate { };
        public event Action<float> TurnTimerStarted = delegate { };
        public event Action<GameState> GameStateUpdated = delegate { };
        public event Action<MoveOrder> OrderUpdated = delegate { };
        public event Action<MoveOrder> OrderSubmitted = delegate { };

        public GameState GameState => gameState;

        public void InitializeGameState(GameState initialState)
        {
            sync.GameState = gameState = initialState;
        }

        private void Awake()
        {
            sync = GetComponent<TurnSyncManager>();
            turnAnimator = GetComponent<TurnAnimator>();

            sync.OnTurnStarted += OnTurnStartedFromServer;
            sync.OnTurnResolved += OnTurnResolvedFromServer;
            sync.OnHeartbeatReceived += OnHeartbeat;
            sync.OnFullStateReceived += OnFullStateReceived;
            sync.OnGameOver += winner =>
            {
                gameState.GameOver = true;
                gameState.WinnerTeamId = winner;

                GameStateUpdated(gameState);
            };
        }

        /* =========================
         * CLIENT INPUT
         * ========================= */

        public void SetLocalOrder(MoveOrder order)
        {
            if (order.TurnIndex != TurnIndex)
                return;

            LocalOrder = order;
            OrderUpdated(LocalOrder.Value);
        }

        public void ConfirmTurn()
        {
            if (!LocalOrder.HasValue)
                LocalOrder = CreatePassOrder();

            sync.SendOrder(LocalOrder.Value);
            OrderSubmitted(LocalOrder.Value);
        }

        /* =========================
         * SERVER EVENTS
         * ========================= */
        private void OnHeartbeat(int serverTurn, float timeLeft, int serverHash)
        {
            if (isDesynced)
                return;

            if (serverTurn != TurnIndex)
            {
                TriggerDesync("TurnIndex mismatch");
                return;
            }

            int localHash = GameStateHasher.Compute(gameState);

            if (localHash != serverHash)
            {
                TriggerDesync("GameState hash mismatch");
                return;
            }

            // Мягкая коррекция таймера
            TurnTimerStarted(timeLeft);
        }

        private void TriggerDesync(string reason)
        {
            Debug.LogWarning($"DESYNC: {reason}");
            isDesynced = true;

            // 1. Блокируем ввод
            // UI/Input system сам подпишется
            DesyncDetected?.Invoke();

            // 2. Запрашиваем полный стейт
            sync.RequestFullStateSyncServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        public event Action DesyncDetected = delegate { };

        private void OnFullStateReceived(GameStateSnapshot state, int turn, float timeLeft)
        {
            var newState = state.FromSnapshot(gameState.Board);
            turnAnimator.AnimateDiff(gameState, newState);

            gameState = newState;
            TurnIndex = turn;
            isDesynced = false;

            GameStateUpdated(gameState);
            TurnTimerStarted(timeLeft);
        }

        private void OnTurnStartedFromServer(int turnIndex, float duration)
        {
            TurnIndex = turnIndex;
            LocalOrder = null;
            OrderUpdated(CreatePassOrder());

            TurnTimerStarted(duration);
            TurnStarted(gameState);
        }

        private void OnTurnResolvedFromServer(MoveOrder a, MoveOrder b)
        {
            var oldState = gameState;

            sync.GameState = gameState = TurnResolver.Resolve(gameState, a, b);
            GameStateUpdated(gameState);
            turnAnimator.AnimateDiff(oldState, gameState);

            TurnFinished(gameState);
        }

        private MoveOrder CreatePassOrder()
        {
            return new MoveOrder
            {
                TeamId = MatchmakingManager.Instance.LocalTeamId,
                TurnIndex = TurnIndex,
                Type = OrderType.Pass
            };
        }
    }
}
