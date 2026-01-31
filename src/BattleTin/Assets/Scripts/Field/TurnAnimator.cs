using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public class TurnAnimator : MonoBehaviour
    {
        [SerializeField] private float moveDuration = 0.5f;

        private Dictionary<int, PieceView> pieceViews;

        public event Action BattleAnimationStarted = delegate { };
        public event Action BattleAnimationFinished = delegate { };

        public event Action<PieceState> PieceMoveStarted = delegate { };


        public void LoadPieceViews(Dictionary<int, PieceView> pieceViews)
        {
            Debug.Log("Piece views were loaded successfully.");
            this.pieceViews = pieceViews;
        }

        public void PlayBattle(
            MoveOrder a,
            MoveOrder b,
            GameState before,
            GameState after
        )
        {
            BattleAnimationStarted?.Invoke();
            // TODO: animate fights
            BattleAnimationFinished?.Invoke();
        }

        public void AnimateDiff(GameState oldState, GameState newState)
        {
            Debug.Log("End-turn animation is running.");
            AnimateMoves(oldState, newState);
            AnimateDeaths(oldState, newState);
        }

        void AnimateMoves(GameState oldState, GameState newState)
        {
            Debug.Log($"Old state: #{oldState.TurnIndex} {string.Join(", ", oldState.Pieces.Values.Select(x => $"{x.PieceId.Value} at {x.CellId.Value}"))}");
            Debug.Log($"New state: #{newState.TurnIndex} {string.Join(", ", newState.Pieces.Values.Select(x => $"{x.PieceId.Value} at {x.CellId.Value}"))}");

            foreach (var kv in newState.Pieces)
            {
                int pieceId = kv.Key;
                var newPiece = kv.Value;

                if (!oldState.Pieces.TryGetValue(pieceId, out var oldPiece))
                    continue;

                if (oldPiece.CellId != newPiece.CellId)
                {
                    Debug.Log($"Piece #{newPiece.PieceId.Value} should move from cell {oldPiece.CellId.Value} to cell {newPiece.CellId.Value}");
                    var view = pieceViews[pieceId];
                    var targetHex = HexByCellId(newPiece.CellId.Value);

                    var direction = targetHex.transform.position - view.transform.position;
                    direction.y = 0; // чтобы не наклонялся вверх/вниз

                    direction = -direction;

                    view.transform
                    .DORotateQuaternion(Quaternion.LookRotation(direction), 0.2f)
                    .SetEase(Ease.OutQuad);


                    var animator = view.GetComponent<Animator>();

                    animator?.SetBool("Moving", true);
                    PieceMoveStarted(newPiece);

                    view.transform.DOMove(
                        targetHex.transform.position + Vector3.up * 0.5f,
                        moveDuration
                    ).OnComplete(() =>
                    {
                        animator?.SetBool("Moving", false);
                    });


                    view.transform.parent = targetHex.transform;
                }
            }
        }

        void AnimateDeaths(GameState oldState, GameState newState)
        {
            foreach (var kv in oldState.Pieces)
            {
                int pieceId = kv.Key;

                if (!newState.Pieces.ContainsKey(pieceId))
                {
                    var view = pieceViews[pieceId];

                    view.transform
                        .DOScale(Vector3.zero, 0.3f)
                        .OnComplete(() =>
                        {
                            Destroy(view.gameObject);
                            pieceViews.Remove(pieceId);
                        });
                }
            }
        }

        Hex HexByCellId(int cellId)
        {
            return HexSelectionManager.Instance
                .GetHexByCellId(cellId);
        }

    }
}
