using System;
using System.Collections.Generic;
using System.Text;

namespace MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode
{
    [Serializable]
    public struct TurnMeta
    {
        public int TurnIndex;
        public double TurnStartServerTime;
        public float TurnDuration; // 30
    }
}
