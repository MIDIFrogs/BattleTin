using System;
using System.Collections.Generic;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    [CreateAssetMenu(menuName = "Masks/Mask Inventory")]
    public class MasksInventoryModel : ScriptableObject
    {
        [SerializeField] private MaskCountModel[] maskCounts;

        public IReadOnlyList<MaskCountModel> MaskCounts => maskCounts;
    }

    [Serializable]
    public class MaskCountModel
    {
        [SerializeField] private MaskType maskType;
        [SerializeField] private int maskCount;

        public MaskType MaskType => maskType;

        public int MaskCount => maskCount;
    }
}
