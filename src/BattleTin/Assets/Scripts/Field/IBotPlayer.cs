using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;

namespace MIDIFrogs.BattleTin.Field
{
    public interface IBotPlayer
    {
        MoveOrder DecideOrder(GameState state, int turnIndex, int teamId);
    }

}
