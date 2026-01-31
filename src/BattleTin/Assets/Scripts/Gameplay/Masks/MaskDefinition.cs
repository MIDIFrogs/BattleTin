using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    public sealed class MaskDefinition
    {
        public MaskType Type;

        public int MaxHealth;
        public int BaseDamage;

        public IMovementRule MovementRule;

        public bool IsKing;
    }
}
