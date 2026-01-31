using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Core;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public class HexSelectionManager : MonoBehaviour
    {
        public static HexSelectionManager Instance;

        public MonoBehaviour playerContextBehaviour;
        private IPlayerContext playerContext;

        public MonoBehaviour turnControllerBehaviour;

        private ITurnController turnController;
        public TurnAnimator turnAnimator;

        private BoardGraph graph;
        private Hex[] allCells;

        public GameObject playerUnitPrefab;
        private PieceView currentSelectedUnit;
        private Hex currentSelectedHex;
        private List<Hex> availableMoveHexes = new List<Hex>();

        private MoveOrder? pendingOrder;

        void Awake()
        {
            turnController = turnControllerBehaviour.GetComponent<ITurnController>();

            if (turnController == null)
                Debug.LogError("Assigned controller does not implement ITurnController");

            playerContext = playerContextBehaviour.GetComponent<IPlayerContext>();

            if (playerContext == null)
                Debug.LogError("Assigned object does not implement IPlayerContext");




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
                        Hp = 1,
                        Mask = 0,
                    });
                }
            }

            turnController.InitializeGameState(GameState.Create(pieceStates, graph));
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void OnHexClicked(Hex clickedHex)
        {
            Debug.Log(string.Join("", availableMoveHexes));
            Debug.Log(clickedHex);
            if (currentSelectedUnit != null && availableMoveHexes.Contains(clickedHex))
            {
                MoveUnitToHex(clickedHex);
                ClearAvailableMoves();
                return;
            }
            if (clickedHex.transform.childCount > 0 && clickedHex.transform.GetChild(0).TryGetComponent<PieceView>(out var piece) && piece.TeamId == playerContext.LocalTeamId)
            {
                Debug.Log($"Clicked own piece! PieceId: {piece.PieceId}, TeamId: {piece.TeamId}");

                SelectUnit(piece, clickedHex);
                ShowAvailableMoves(clickedHex);
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
        }

        void ShowAvailableMoves(Hex fromHex)
        {
            availableMoveHexes.Clear();

            availableMoveHexes.AddRange(
                allCells.Where(x => graph.IsDirectConnected(fromHex.CellId, x.CellId))
            );

            foreach (var hex in availableMoveHexes)
            {
                hex.GetComponent<Button3DHighlight>()?.Select();
            }
        }

        void MoveUnitToHex(Hex targetHex)
        {
            if (currentSelectedUnit == null) return;

            turnController.SetLocalOrder(
                MoveOrder.Move(turnController.TurnIndex, playerContext.LocalTeamId, currentSelectedUnit.PieceId, targetHex.CellId)
            );

            currentSelectedHex.GetComponent<Button3DHighlight>().Deselect();
            currentSelectedHex = targetHex;
            currentSelectedHex.GetComponent<Button3DHighlight>().Select(); // Теперь она выбрана на новом месте
        }

        void ClearAvailableMoves()
        {
            foreach (Hex hex in availableMoveHexes)
            {
                hex.GetComponent<Button3DHighlight>().Deselect();
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