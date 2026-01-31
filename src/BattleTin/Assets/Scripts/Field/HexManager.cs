using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Core;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Masks;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public class HexSelectionManager : MonoBehaviour
    {
        public static HexSelectionManager Instance;

        public TurnController turnController;
        public TurnAnimator turnAnimator;
        [SerializeField] MasksInventoryModel maskInventory;

        private BoardGraph graph;
        private Hex[] allCells;

        public GameObject playerUnitPrefab;
        private PieceView currentSelectedUnit;
        private Hex currentSelectedHex;
        private List<Hex> availableMoveHexes = new List<Hex>();

        public PieceView SelectedPiece => currentSelectedUnit;

        public event Action<PieceView> SelectionUpdated = delegate { };

        void Awake()
        {
            allCells = transform.GetComponentsInChildren<Hex>();
            var pieces = transform.GetComponentsInChildren<PieceView>();
            graph = new(allCells.Length);
            int index = 0;
            foreach (var cell in allCells)
            {
                if (cell.CellId == 0)
                    cell.CellId = index++;
            }
            index = 0;
            foreach (var p in pieces)
            {
                if (p.PieceId == 0)
                    p.PieceId = index++;
            }
            turnAnimator.LoadPieceViews(pieces.ToDictionary(x => x.PieceId, y => y));

            List<PieceState> pieceStates = new();
            foreach (var cell in allCells)
            {
                graph.AddConnections(new(cell.CellId), cell.Neighbors.Select(x => new CellId(x.CellId)), cell.DiagonalNeighbors.Select(x => new CellId(x.CellId)));
                var piece = cell.transform.GetComponentInChildren<PieceView>();
                if (piece != null)
                {
                    pieceStates.Add(new()
                    {
                        CellId = cell.CellId,
                        PieceId = piece.PieceId,
                        TeamId = piece.TeamId,
                        Health = 1,
                        MaxHealth = 1,
                        Mask = piece.IsKing ? MaskType.Captain : MaskType.None,
                    });
                }
            }

            List<MaskInventory> inventories = new()
            {
                new() { TeamId = 0 },
                new() { TeamId = 1 },
            };
            foreach (var m in maskInventory.MaskCounts)
            {
                inventories[0].Counts[m.MaskType] = inventories[1].Counts[m.MaskType] = m.MaskCount;
            }

            turnController.InitializeGameState(GameState.Create(pieceStates, inventories, graph));
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void OnHexClicked(Hex clickedHex)
        {
            if (currentSelectedUnit != null && availableMoveHexes.Contains(clickedHex))
            {
                MoveUnitToHex(clickedHex);
                ClearAvailableMoves();
                return;
            }

            if (clickedHex.transform.childCount == 0)
            {
                ClearSelection();
                return;
            }

            var pieceViews = clickedHex.transform.GetComponentsInChildren<PieceView>();
            var piece = pieceViews.FirstOrDefault(x => x.TeamId == MatchmakingManager.Instance.LocalTeamId);

            if (piece != null)
            {
                SelectUnit(piece, clickedHex);
                ShowAvailableMoves(clickedHex);

                Debug.Log(string.Join(",", availableMoveHexes));
                Debug.Log(clickedHex.gridCoordinates);
            }
            else
            {
                ClearSelection();
            }
        }

        public Hex GetHexByCellId(int id)
        {
            return allCells.First(x => x.CellId == id);
        }

        void SelectUnit(PieceView unit, Hex hex)
        {
            ClearSelection();
            currentSelectedUnit = unit;
            currentSelectedHex = hex;
            hex.GetComponent<Button3DHighlight>().Select(); // Подсветили гекс под фигуркой
                                                            // Можно добавить эффект на саму фигурку
            SelectionUpdated(currentSelectedUnit);
        }

        void ShowAvailableMoves(Hex fromHex)
        {
            var piece = turnController.GameState.Pieces[SelectedPiece.PieceId];
            Debug.Log($"Showing available moves for Piece ID: {SelectedPiece.PieceId}");

            var movePattern = MaskDatabase.All[piece.Mask].MovementRule;
            Debug.Log($"Movement Pattern for Piece ID {SelectedPiece.PieceId}: {movePattern}");

            var available = movePattern.GetAvailableCells(piece, turnController.GameState).ToList();
            Debug.Log($"Available Cells: {string.Join(", ", available)}");

            availableMoveHexes.AddRange(allCells.Where(x => available.Contains(x.CellId)));
            Debug.Log($"Hexes to highlight: {string.Join(", ", availableMoveHexes.Select(h => h.CellId))}");

            foreach (var hex in availableMoveHexes)
            {
                hex.GetComponent<Button3DHighlight>().Highlight();
                Debug.Log($"Highlighted Hex: {hex.CellId}");
            }
        }


        void MoveUnitToHex(Hex targetHex)
        {
            if (currentSelectedUnit == null) return;

            turnController.SetLocalOrder(
                MoveOrder.Move(turnController.TurnIndex, MatchmakingManager.Instance.LocalTeamId, currentSelectedUnit.PieceId, targetHex.CellId)
            );

            currentSelectedHex.GetComponent<Button3DHighlight>().Deselect();
            currentSelectedHex = targetHex;
            currentSelectedHex.GetComponent<Button3DHighlight>().Select(); // Теперь она выбрана на новом месте
        }

        void ClearAvailableMoves()
        {
            foreach (Hex hex in availableMoveHexes)
            {
                var btn = hex.GetComponent<Button3DHighlight>();
                btn.Deselect();
                btn.RemoveHighlight();
            }
            availableMoveHexes.Clear();
        }

        void ClearSelection()
        {
            if (currentSelectedHex != null) currentSelectedHex.GetComponent<Button3DHighlight>().Deselect();
            currentSelectedUnit = null;
            currentSelectedHex = null;
            ClearAvailableMoves();
        }
    }
}