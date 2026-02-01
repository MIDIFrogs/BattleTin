using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Orders;

namespace MIDIFrogs.BattleTin.Field
{
    public interface IBotPlayer
    {
        MoveOrder DecideOrder(GameState state, MoveOrder localOrder, int turnIndex, int teamId);
    }
}
