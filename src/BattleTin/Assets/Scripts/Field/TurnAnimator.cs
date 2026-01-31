using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MIDIFrogs.BattleTin.Gameplay;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public class TurnAnimator : MonoBehaviour
    {
        [SerializeField] private float moveDuration = 0.5f;

        private Dictionary<int, PieceView> pieceViews;

        public void LoadPieceViews(Dictionary<int, PieceView> pieceViews)
        {
            Debug.Log("Piece views were loaded successfully.");
            this.pieceViews = pieceViews;
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

                    view.transform.DOMove(
                        targetHex.transform.position + Vector3.up * 0.5f,
                        moveDuration
                    );

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
