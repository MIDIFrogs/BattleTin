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
    public class TurnController : TurnControllerBase
    {
        private TurnAnimator turnAnimator;
        private TurnSyncManager sync;

        private bool isDesynced;

        public MoveOrder? LocalOrder { get; private set; }
        public override int TurnIndex { get; protected set; }

        public override GameState GameState { get; protected set; }

        public override void InitializeGameState(GameState initialState)
        {
            sync.GameState = GameState = initialState;
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
                GameState.GameOver = true;
                GameState.WinnerTeamId = winner;

                OnGameStateUpdated(GameState);
            };
        }

        /* =========================
         * CLIENT INPUT
         * ========================= */

        public override void SetLocalOrder(MoveOrder order)
        {
            if (order.TurnIndex != TurnIndex)
                return;

            LocalOrder = order;
            OnOrderUpdated(LocalOrder.Value);
        }

        public override void ConfirmTurn()
        {
            if (!LocalOrder.HasValue)
                LocalOrder = CreatePassOrder();

            sync.SendOrder(LocalOrder.Value);
            OnOrderSubmitted(LocalOrder ?? CreatePassOrder());
        }

        public void Surrender()
        {
            sync.SurrenderServerRpc(MatchmakingManager.Instance.LocalTeamId);
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

            int localHash = GameStateHasher.Compute(GameState);

            if (localHash != serverHash)
            {
                TriggerDesync("GameState hash mismatch");
                return;
            }

            // Мягкая коррекция таймера
            OnTurnTimerStarted(timeLeft);
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
            var newState = state.FromSnapshot(GameState.Board);
            turnAnimator.AnimateDiff(GameState, newState);

            GameState = newState;
            TurnIndex = turn;
            isDesynced = false;

            OnGameStateUpdated(GameState);
            OnTurnTimerStarted(timeLeft);
        }

        private void OnTurnStartedFromServer(int turnIndex, float duration)
        {
            TurnIndex = turnIndex;
            LocalOrder = null;
            OnOrderUpdated(CreatePassOrder());

            OnTurnTimerStarted(duration);
            OnTurnStarted(GameState);
        }

        private void OnTurnResolvedFromServer(MoveOrder a, MoveOrder b)
        {
            var oldState = GameState;

            sync.GameState = GameState = TurnResolver.Resolve(GameState, a, b);
            OnGameStateUpdated(GameState);
            turnAnimator.AnimateDiff(oldState, GameState);
            turnAnimator.PlayBattle(a, b, oldState, GameState);

            OnTurnFinished(GameState);
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
