using MIDIFrogs.BattleTin.Core;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    [RequireComponent(typeof(TurnAnimator))]
    [RequireComponent(typeof(Button3DHighlight))]
    public class OfflineTurnController : MonoBehaviour, ITurnController
    {
        private GameState gameState;
        private TurnAnimator turnAnimator;
        private Button3DHighlight button;

        public MoveOrder? LocalOrder { get; private set; }
        public TurnPhase Phase { get; private set; } = TurnPhase.Planning;
        public int TurnIndex { get; private set; }

        [SerializeField] private int localTeamId = 0;

        public void InitializeGameState(GameState initialState)
        {
            gameState = initialState;
        }

        private void Awake()
        {
            turnAnimator = GetComponent<TurnAnimator>();
            button = GetComponent<Button3DHighlight>();
        }

        public void SetLocalOrder(MoveOrder order)
        {
            if (Phase != TurnPhase.Planning)
                return;

            LocalOrder = order;
            ConfirmTurn();
        }

        public void ConfirmTurn()
        {
            if (!LocalOrder.HasValue)
                LocalOrder = CreatePassOrder();

            Phase = TurnPhase.Resolving;

            ResolveTurn();
        }

        private MoveOrder CreatePassOrder() => new()
        {
            Mask = Gameplay.Pieces.MaskType.None,
            PieceId = 0,
            Type = OrderType.Pass,
            TeamId = localTeamId,
            TargetCellId = 0,
            TurnIndex = TurnIndex,
        };

        private void ResolveTurn()
        {
            var oldState = gameState;

            var newState = TurnResolver.Resolve(
                gameState,
                LocalOrder.Value,
                CreatePassOrder()   // противник всегда пасует
            );

            gameState = newState;

            turnAnimator.AnimateDiff(oldState, newState);

            FinishTurn();
        }

        void FinishTurn()
        {
            LocalOrder = null;
            TurnIndex++;
            Phase = TurnPhase.Planning;
            button?.Deselect();
        }
    }
}
