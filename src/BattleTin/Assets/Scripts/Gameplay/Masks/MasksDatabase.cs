using System.Collections.Generic;
using MIDIFrogs.BattleTin.Gameplay.Masks;
using MIDIFrogs.BattleTin.Gameplay.Pieces;

namespace MIDIFrogs.BattleTin.Gameplay.Masks
{
    public static class MaskDatabase
    {
        public static readonly Dictionary<MaskType, MaskDefinition> All =
            new()
            {
                [MaskType.None] = new()
                {
                    Type = MaskType.None,
                    MaxHealth = 1,
                    BaseDamage = 1,
                    MovementRule = new DirectStepRule(1),
                },

                [MaskType.SeaWolf] = new()
                {
                    Type = MaskType.SeaWolf,
                    MaxHealth = 2,
                    BaseDamage = 2,
                    MovementRule = new ExclusiveDirectStepRule(2),
                },

                [MaskType.Cook] = new()
                {
                    Type = MaskType.Cook,
                    MaxHealth = 2,
                    BaseDamage = 1,
                    MovementRule = new DiagonalStepRule(1),
                },

                [MaskType.Rat] = new()
                {
                    Type = MaskType.Rat,
                    MaxHealth = 1,
                    BaseDamage = 2,
                    MovementRule = new ExclusiveDiagonalStepRule(2),
                },

                [MaskType.Parrot] = new()
                {
                    Type = MaskType.Parrot,
                    MaxHealth = 3,
                    BaseDamage = 0,
                    MovementRule = new FreeTeleportRule(),
                },

                [MaskType.Cannoneer] = new()
                {
                    Type = MaskType.Cannoneer,
                    MaxHealth = 1,
                    BaseDamage = 1,
                    MovementRule = new JumpOverRule(),
                },

                [MaskType.Carpenter] = new()
                {
                    Type = MaskType.Carpenter,
                    MaxHealth = 2,
                    BaseDamage = 1,
                    MovementRule = new DirectStepRule(1, everyNTurns: 2),
                },

                [MaskType.Captain] = new()
                {
                    Type = MaskType.Captain,
                    MaxHealth = 5,
                    BaseDamage = 3,
                    MovementRule = new KingMoveRule(),
                    IsKing = true,
                },

                [MaskType.Barricade] = new()
                {
                    Type = MaskType.Barricade,
                    MaxHealth = 1,
                    BaseDamage = 0,
                    MovementRule = new NoMoveRule(),
                }
            };
    }
}
