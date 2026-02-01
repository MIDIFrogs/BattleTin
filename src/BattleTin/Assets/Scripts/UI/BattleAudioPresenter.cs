using System.Threading;
using Cysharp.Threading.Tasks;
using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI
{
    public class BattleAudioPresenter : MonoBehaviour
    {
        [SerializeField] private BattleAudioManager audioManager;
        [SerializeField] private TurnController turnController;
        [SerializeField] private TurnAnimator turnAnimator;
        [SerializeField] private HexSelectionManager hexManager;

        private int initialUnitCount;
        private bool played25;
        private bool played50;
        private bool gameOverHandled;

        private CancellationTokenSource turnTimerCts;

        private void Awake()
        {
            turnController.GameStateUpdated += OnGameStateUpdated;
            turnController.TurnTimerStarted += OnTurnTimerStarted;
            hexManager.SelectionUpdated += OnPieceSelected;
            turnAnimator.BattleAnimationStarted += OnBattleStarted;
            turnAnimator.PieceMoveStarted += OnMoveStarted;
        }

        private void OnMoveStarted(Gameplay.Pieces.PieceState obj)
        {
            var piece = turnController.GameState.Pieces[obj.PieceId.Value];
            switch (piece.Mask)
            {
                case Gameplay.Pieces.MaskType.None:
                    audioManager.PlaySalagaWalk(); break;
                case Gameplay.Pieces.MaskType.SeaWolf:
                    audioManager.PlaySeaWolfWalk(); break;
                case Gameplay.Pieces.MaskType.Cannoneer:
                    audioManager.PlayCorsairWalk(); break;
                case Gameplay.Pieces.MaskType.Parrot:
                    audioManager.PlayParrotWalk(); break;
                case Gameplay.Pieces.MaskType.Captain:
                    audioManager.PlayCaptainWalk(); break;
                case Gameplay.Pieces.MaskType.Carpenter:
                    audioManager.PlayCarpenterWalk(); break;
                case Gameplay.Pieces.MaskType.Cook:
                    audioManager.PlayCookWalk(); break;
                case Gameplay.Pieces.MaskType.Rat:
                    audioManager.PlayRatWalk(); break;
            }
        }

        private void OnPieceSelected(PieceView obj)
        {
            var piece = turnController.GameState.Pieces[obj.PieceId];
            switch (piece.Mask)
            {
                case Gameplay.Pieces.MaskType.None:
                    audioManager.PlaySalagaPickUp(); break;
                case Gameplay.Pieces.MaskType.SeaWolf:
                    audioManager.PlaySeaWolfPickUp(); break;
                case Gameplay.Pieces.MaskType.Cannoneer:
                    audioManager.PlayCorsairPickUp(); break;
                case Gameplay.Pieces.MaskType.Parrot:
                    audioManager.PlayParrotPickUp(); break;
                case Gameplay.Pieces.MaskType.Captain:
                    audioManager.PlayCaptainPickUp(); break;
                case Gameplay.Pieces.MaskType.Carpenter:
                    audioManager.PlayCarpenterPickUp(); break;
                case Gameplay.Pieces.MaskType.Cook:
                    audioManager.PlayCookPickUp(); break;
                case Gameplay.Pieces.MaskType.Rat:
                    audioManager.PlayRatPickUp(); break;
            }
        }

        private void OnDestroy()
        {
            turnTimerCts?.Cancel();
        }

        private void OnGameStateUpdated(GameState state)
        {
            if (initialUnitCount == 0)
                initialUnitCount = state.Pieces.Count;

            int alive = state.Pieces.Count;
            int killed = initialUnitCount - alive;
            float killedRatio = (float)killed / initialUnitCount;

            //if (!played25 && killedRatio >= 0.25f)
            //{
            //    played25 = true;
            //    audioManager.PlayThemeBattleSecond();
            //}

            //if (!played50 && killedRatio >= 0.5f)
            //{
            //    played50 = true;
            //    audioManager.PlayThemeBattleThird();
            //}

            if (state.GameOver && !gameOverHandled)
            {
                gameOverHandled = true;

                if (state.WinnerTeamId == MatchmakingManager.Instance.LocalTeamId)
                {
                    audioManager.PlayResultWin();
                    audioManager.PlayBattleWinningSoundEnd();
                }
                else
                {
                    audioManager.PlayResultLose();
                    audioManager.PlayBattleLosingSoundEnd();
                }
            }
        }

        private void OnTurnTimerStarted(float duration)
        {
            turnTimerCts?.Cancel();
            turnTimerCts = new CancellationTokenSource();

            PlayEndTurnSoundAsync(duration, turnTimerCts.Token).Forget();
        }

        private async UniTaskVoid PlayEndTurnSoundAsync(float duration, CancellationToken token)
        {
            if (duration <= 15f)
                return;

            await UniTask.Delay(
                (int)((duration - 15f) * 1000),
                cancellationToken: token
            );

            audioManager.PlayEndTurnSound();
        }

        private void OnBattleStarted()
        {
            audioManager.PlayBattleSound();
        }
    }

}
