using System.Collections.Generic;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay
{
    public class MaskInventory
    {
        public int TeamId;
        public Dictionary<MaskType, int> Counts = new();
    }
}
