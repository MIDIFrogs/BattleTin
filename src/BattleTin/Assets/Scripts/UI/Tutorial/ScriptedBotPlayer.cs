using System.Linq;
using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Masks;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using MIDIFrogs.FutureInThePast.UI.Dialogs;
using UnityEngine;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Tutorial
{
    public sealed class ScriptedBotPlayer : IBotPlayer
    {
        private DialogPlayer _dialogPlayer;
        private Dialog[] _dialogs;

        private int phase;

        public ScriptedBotPlayer(DialogPlayer dialogPlayer, Dialog[] dialogs)
        {
            _dialogPlayer = dialogPlayer;
            _dialogs = dialogs;
        }

        public MoveOrder DecideOrder(GameState state, MoveOrder localOrder, int turnIndex, int teamId)
        {
            Debug.Log($"DecideOrder called - TeamId: {teamId}, TurnIndex: {turnIndex}, Current Phase: {phase}");

            switch (phase)
            {
                case 0:
                    Debug.Log("Checking for opponent pieces with last action turn not equal to 0.");
                    if (localOrder.Type == OrderType.Move)
                    {
                        Debug.Log($"Opponent pieces found. Clearing inventory for team {1 - teamId}.");
                        state.Inventories[1 - teamId].Counts.Clear();
                        state.Inventories[1 - teamId].Counts[MaskType.SeaWolf] = 1;
                        _dialogPlayer.StartDialog(_dialogs[phase = 1]);
                        Debug.Log($"Dialog started for phase 1. Passing turn for team {teamId}.");
                        return CreatePass(turnIndex, teamId);
                    }
                    break;
                case 1:
                    Debug.Log("Checking for opponent pieces with SeaWolf mask.");
                    if (localOrder.Type == OrderType.EquipMask && localOrder.Mask == MaskType.SeaWolf)
                    {
                        Debug.Log("Opponent piece with SeaWolf mask found. Adding barricade piece.");
                        state.Pieces.Add(-1, new PieceState()
                        {
                            CellId = 21,
                            Mask = MaskType.Barricade,
                            Health = 1,
                            MaxHealth = 1,
                            PieceId = -1,
                            TeamId = teamId,
                        });
                        _dialogPlayer.StartDialog(_dialogs[phase = 2]);
                        Debug.Log($"Dialog started for phase 2. Passing turn for team {teamId}.");
                        return CreatePass(turnIndex, teamId);
                    }
                    break;
                case 2:
                    Debug.Log("Checking for opponent to kill the barricade.");
                    if (!TurnResolver.Resolve(state, localOrder, CreatePass(turnIndex, teamId)).Pieces.Any(x => x.Value.Mask == MaskType.Barricade))
                    {
                        state.Inventories[1 - teamId].Counts.Clear();
                        state.Inventories[1 - teamId].Counts[MaskType.Cook] = 1;
                        _dialogPlayer.StartDialog(_dialogs[phase = 3]);
                        Debug.Log($"Dialog started for phase 3. Passing turn for team {teamId}.");
                        return CreatePass(turnIndex, teamId);
                    }
                    break;
                case 3:
                    if (localOrder.Type == OrderType.EquipMask && localOrder.Mask == MaskType.Cook)
                    {
                        _dialogPlayer.StartDialog(_dialogs[phase = 4]);
                        Debug.Log($"Dialog started for phase 4. Passing turn for team {teamId}.");
                        return CreatePass(turnIndex, teamId);
                    }
                    break;
                case 4:
                    var king = state.Pieces.Values.FirstOrDefault(x => x.TeamId == teamId && MaskDatabase.All[x.Mask].IsKing);
                    if (king == null) return CreatePass(turnIndex, teamId);
                    return CreateRandomMove(state, king, turnIndex, teamId);
                default:
                    Debug.Log("No conditions met for advancing phase. Passing turn.");
                    break;
            }

            Debug.Log($"Final action: Passing turn for team {teamId}.");
            return CreatePass(turnIndex, teamId);
        }

        private MoveOrder CreateRandomMove(GameState state, PieceState king, int turn, int team) => new()
        {
            PieceId = king.PieceId.Value,
            TargetCellId = MaskDatabase.All[MaskType.Captain].MovementRule.GetAvailableCells(king, state).OrderBy(_ => Random.value).First().Value,
            TeamId = team,
            TurnIndex = turn,
            Type = OrderType.Move,
        };

        private MoveOrder CreatePass(int turn, int team)
        {
            return new MoveOrder
            {
                TurnIndex = turn,
                TeamId = team,
                Type = OrderType.Pass
            };
        }
    }
}
