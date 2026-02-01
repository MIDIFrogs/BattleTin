using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using MIDIFrogs.FutureInThePast.UI.Dialogs;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Tutorial
{
    public sealed class ScriptedBotPlayer : IBotPlayer
    {
        private DialogPlayer _dialogPlayer;
        private Dialog[] _dialogs;

        public ScriptedBotPlayer(DialogPlayer dialogPlayer, Dialog[] dialogs)
        {
            _dialogPlayer = dialogPlayer;
            _dialogs = dialogs;
        }

        public MoveOrder DecideOrder(GameState state, int turnIndex, int teamId)
        {
            if (turnIndex == 0)
            {
                _dialogPlayer.StartDialog(_dialogs[1]);
                return CreatePass(turnIndex, teamId);
            }

            return CreatePass(turnIndex, teamId);
        }

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
