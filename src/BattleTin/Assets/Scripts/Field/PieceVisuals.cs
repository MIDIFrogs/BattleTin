using System;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field.Assets.Scripts.Field
{
    [Serializable]
    public class PieceVisuals : ScriptableObject
    {
        public PieceConfig[] Pieces;
    }

    [Serializable]
    public class PieceConfig
    {
        public MaskType Mask;
        public int TeamIndex;
        public GameObject Visual;
    }
}
