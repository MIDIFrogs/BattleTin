using System;
using System.Collections.Generic;
using System.Text;

namespace MIDIFrogs.BattleTin.Gameplay.Pieces
{
    [Serializable]
    public enum MaskType
    {
        None = 0,

        // Pirates set
        SeaWolf,        // Морской волк
        Cook,           // Кок
        Rat,            // Корабельная крыса
        Parrot,         // Попугай
        Cannoneer,      // Канонир
        Carpenter,
        Captain,
        Barricade,

        // Rainforests set
        // TODO
    }
}
