using System;
using System.Collections.Generic;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Netcode
{
    public class TurnSyncManager : NetworkBehaviour
    {
        public const float TURN_DURATION = 30f;
        public const float TURN_GRACE = 5f;
        public const float HEARTBEAT_INTERVAL = 1.0f;

        private float nextHeartbeatTime;
        private int lastSentStateHash;

        private readonly Dictionary<int, MoveOrder> ordersByTeam = new();

        public int CurrentTurnIndex { get; private set; }
        private float turnDeadline;

        public GameState GameState { get; set; }

        // === Client events ===
        public event Action<int, float> OnTurnStarted = delegate { };
        public event Action<MoveOrder> OnOrderConfirmed = delegate { };
        public event Action<MoveOrder, MoveOrder> OnTurnResolved = delegate { };

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                StartNewTurn();
            }

            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void FinishGame(int winnerTeamId)
        {
            GameOverClientRpc(winnerTeamId);
        }


        /* =========================
         * CLIENT -> SERVER
         * ========================= */

        public void SendOrder(MoveOrder order)
        {
            if (!IsClient)
                return;

            SubmitOrderServerRpc(order);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SubmitOrderServerRpc(ForceNetworkSerializeByMemcpy<MoveOrder> order)
        {
            if (order.Value.TurnIndex != CurrentTurnIndex)
                return;

            if (ordersByTeam.ContainsKey(order.Value.TeamId))
                return;

            ordersByTeam[order.Value.TeamId] = order;

            OrderConfirmedClientRpc(order);

            if (ordersByTeam.Count >= 2)
                ResolveTurnServer();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestFullStateSyncServerRpc(ulong clientId)
        {
            FullStateSyncClientRpc(
                GameState.ToSnapshot(),
                CurrentTurnIndex,
                turnDeadline - Time.time,
                clientId
            );
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SurrenderServerRpc(int teamId)
        {
            if (GameState.GameOver)
                return;

            FinishGame(winnerTeamId: 1 - teamId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer)
                return;

            int teamId = clientId == NetworkManager.CurrentSessionOwner ? 0 : 1;
            FinishGame(winnerTeamId: 1 - teamId);
        }

        /* =========================
         * SERVER UPDATE
         * ========================= */

        private void Update()
        {
            if (!IsServer)
                return;

            if (Time.time >= turnDeadline)
            {
                ResolveTurnServer();
                return;
            }

            if (Time.time >= nextHeartbeatTime)
            {
                SendHeartbeat();
                nextHeartbeatTime = Time.time + HEARTBEAT_INTERVAL;
            }
        }

        private void SendHeartbeat()
        {
            if (GameState == null) return;

            int stateHash = GameStateHasher.Compute(GameState);

            float timeLeft = Mathf.Max(0, turnDeadline - Time.time);

            HeartbeatClientRpc(
                CurrentTurnIndex,
                timeLeft,
                stateHash
            );
        }

        /* =========================
         * SERVER LOGIC
         * ========================= */

        private void StartNewTurn()
        {
            ordersByTeam.Clear();
            turnDeadline = Time.time + TURN_DURATION + TURN_GRACE;

            TurnStartedClientRpc(CurrentTurnIndex, TURN_DURATION);
        }

        private void ResolveTurnServer()
        {
            // Заполняем пропуски
            for (int teamId = 0; teamId < 2; teamId++)
            {
                if (!ordersByTeam.ContainsKey(teamId))
                {
                    ordersByTeam[teamId] = CreatePassOrder(teamId);
                }
            }

            var orderA = ordersByTeam[0];
            var orderB = ordersByTeam[1];

            TurnResolvedClientRpc(orderA, orderB);

            CurrentTurnIndex++;
            StartNewTurn();
        }

        private MoveOrder CreatePassOrder(int teamId)
        {
            return new MoveOrder
            {
                TeamId = teamId,
                TurnIndex = CurrentTurnIndex,
                Type = OrderType.Pass,
                PieceId = 0,
                TargetCellId = 0,
                Mask = Gameplay.Pieces.MaskType.None
            };
        }

        /* =========================
         * SERVER -> CLIENTS
         * ========================= */
        [ClientRpc]
        private void GameOverClientRpc(int winnerTeamId)
        {
            OnGameOver?.Invoke(winnerTeamId);
        }

        public event Action<int> OnGameOver;

        [ClientRpc]
        private void HeartbeatClientRpc(int turnIndex, float timeLeft, int stateHash)
        {
            OnHeartbeatReceived(turnIndex, timeLeft, stateHash);
        }

        public event Action<int, float, int> OnHeartbeatReceived = delegate { };

        [ClientRpc]
        private void FullStateSyncClientRpc(
            GameStateSnapshot snapshot,
            int turnIndex,
            float timeLeft,
            ulong targetClientId
        )
        {
            if (NetworkManager.Singleton.LocalClientId != targetClientId)
                return;

            OnFullStateReceived(snapshot, turnIndex, timeLeft);
        }


        public event Action<GameStateSnapshot, int, float> OnFullStateReceived;


        [ClientRpc]
        private void TurnStartedClientRpc(int turnIndex, float duration)
        {
            OnTurnStarted(turnIndex, duration);
        }

        [ClientRpc]
        private void OrderConfirmedClientRpc(ForceNetworkSerializeByMemcpy<MoveOrder> order)
        {
            OnOrderConfirmed(order);
        }

        [ClientRpc]
        private void TurnResolvedClientRpc(
            ForceNetworkSerializeByMemcpy<MoveOrder> orderA,
            ForceNetworkSerializeByMemcpy<MoveOrder> orderB
        )
        {
            OnTurnResolved(orderA, orderB);
        }
    }
}
