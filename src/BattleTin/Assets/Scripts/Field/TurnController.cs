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
    [RequireComponent(typeof(Button3DHighlight))]
    public class TurnController : MonoBehaviour, ITurnController
    {
        private GameState gameState;
        private TurnAnimator turnAnimator;
        private TurnSyncManager turnSyncManager;
        private Button3DHighlight button;

        public MoveOrder? LocalOrder { get; private set; }
        public TurnPhase Phase { get; private set; }

        public MoveOrder? RemoteOrder { get; private set; }
        public int TurnIndex { get; private set; }

        public void InitializeGameState(GameState initialState)
        {
            gameState = initialState;
        }

        public void ConfirmTurn()
        {
            if (Phase != TurnPhase.Planning)
                return;

            // TODO: lock the UI

            if (!LocalOrder.HasValue)
            {
                LocalOrder = CreatePassOrder();
            }

            Phase = TurnPhase.WaitingForRemote;
            button.Select();
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
        }

        private void Awake()
        {
            turnSyncManager = GetComponent<TurnSyncManager>();
            turnSyncManager.OnRemoteConfirmed += TryResolve;
            turnAnimator = GetComponent<TurnAnimator>();
            button = GetComponent<Button3DHighlight>();
        }

        void FinishTurn()
        {
            LocalOrder = null;
            RemoteOrder = null;

            TurnIndex++;
            Phase = TurnPhase.Planning;

            // TODO: enable UI
            button.Deselect();
        }

        private void ResolveTurn()
        {
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
            RemoteOrder = obj;
            if (!LocalOrder.HasValue || !RemoteOrder.HasValue)
                return;

            Phase = TurnPhase.Resolving;
            ResolveTurn();
        }
    }
}