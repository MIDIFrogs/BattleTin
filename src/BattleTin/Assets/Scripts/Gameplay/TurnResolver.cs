using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Gameplay.Masks;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Gameplay
{
    public static class TurnResolver
    {
        public static GameState Resolve(
            GameState state,
            MoveOrder a,
            MoveOrder b
        )
        {
            var next = state.Clone();
            next.TurnIndex++;

            // === 1. Собираем перемещения ===
            var moveIntents = new Dictionary<int, int>();
            // pieceId -> targetCellId

            CollectMoveIntent(next, a, moveIntents);
            CollectMoveIntent(next, b, moveIntents);

            // === 2. Применяем перемещения ===
            PerformMoves(state, next, moveIntents);

            // === 3. Меняем маски ===
            ApplyEquipMask(next, a);
            ApplyEquipMask(next, b);

            // === 4. Проводим симуляцию боя ===
            SimulateBattle(next);

            return next;
        }

        private static void SimulateBattle(GameState state)
        {
            var aliveUnits = state.Pieces.Values.Where(u => u.IsAlive).ToList();

            Debug.Log($"Alive units number: {aliveUnits.Count}");

            foreach (var unit in aliveUnits)
            {
                Debug.Log($"Unit #{unit.PieceId.Value}: HP={unit.Health}, TeamId={unit.TeamId}, CellId={unit.CellId.Value}");
            }

            var virtualHp = aliveUnits.ToDictionary(u => u.PieceId, u => u.Health);
            var attackers = aliveUnits
                .OrderBy(t => !t.IsBarricade)
                .ThenBy(t => t.LastActionTurn)
                .ThenBy(t => t.PieceId.Value)
                .ToList();

            var plannedAttacks = new List<PlannedAttack>();

            foreach (var attacker in attackers)
            {
                // Log the attacker's state
                Debug.Log($"Processing attacker #{attacker.PieceId.Value} with HP={virtualHp[attacker.PieceId]}");

                var targets = aliveUnits
                    .Where(t =>
                        t.TeamId != attacker.TeamId &&
                        virtualHp[t.PieceId] > 0 &&
                        CanBeTargeted(t) &&
                        (
                            t.CellId == attacker.CellId ||
                            state.Board.IsDirectConnected(attacker.CellId, t.CellId)
                        )
                    )
                    .OrderBy(t => !t.IsBarricade)
                    .ThenBy(t => t.LastActionTurn)
                    .ThenBy(t => t.PieceId.Value)
                    .ToList();

                // Log target selection information
                Debug.Log($"Attacker #{attacker.PieceId.Value} found {targets.Count} potential targets.");

                if (targets.Count == 0)
                    continue;

                var target = targets[0];

                Debug.Log($"Simulating attack from #{attacker.PieceId.Value} to #{target.PieceId.Value}");

                int damage = CalculateDamage(attacker, state);
                Debug.Log($"Calculated damage from #{attacker.PieceId.Value} to #{target.PieceId.Value}: {damage}");

                plannedAttacks.Add(new PlannedAttack
                {
                    Attacker = attacker,
                    Target = target,
                    Damage = damage
                });


                virtualHp[target.PieceId] -= damage;
                Debug.Log($"Target #{target.PieceId.Value} HP reduced to {virtualHp[target.PieceId]} after attack.");
            }

            foreach (var attack in plannedAttacks)
            {
                if (attack.Damage > 0)
                {
                    attack.Attacker.HasDealtDamage = true;
                }

                attack.Target.Health -= attack.Damage;

                Debug.Log($"Target #{attack.Target.PieceId.Value} final HP after applying damage: {attack.Target.Health}");
            }

            ApplyExplosions(state);

            var toRemove = state.Pieces.Values.Where(u => !u.IsAlive).ToList();

            foreach (var p in toRemove)
            {
                state.Pieces.Remove(p.PieceId.Value);
                Debug.Log($"Removing unit #{p.PieceId.Value} from the field.");
            }

            bool kingDead = toRemove.Any(p => MaskDatabase.All[p.Mask].IsKing);

            if (kingDead)
            {
                state.GameOver = true;

                var deadKings = toRemove.Where(p => MaskDatabase.All[p.Mask].IsKing).ToList();
                if (deadKings.Count > 1)
                    state.WinnerTeamId = -1; // Ничья

                var king = deadKings[0];
                state.WinnerTeamId = king.TeamId == 0 ? 1 : 0; // Выигрывает команда-противник короля
            }
        }

        private static void ApplyExplosions(GameState state)
        {
            var exploded = new HashSet<PieceId>();
            bool anyNewExplosion;

            do
            {
                anyNewExplosion = false;

                var deadCannoneers = state.Pieces.Values
                    .Where(p =>
                        !p.IsAlive &&
                        p.Mask == MaskType.Cannoneer &&
                        !exploded.Contains(p.PieceId)
                    )
                    .ToList();

                foreach (var cannon in deadCannoneers)
                {
                    exploded.Add(cannon.PieceId);
                    anyNewExplosion = true;

                    var neighbors = state.Pieces.Values
                        .Where(p =>
                            p.IsAlive &&
                            state.Board.IsDirectConnected(cannon.CellId, p.CellId)
                        );

                    foreach (var target in neighbors)
                    {
                        target.Health -= 1;
                    }
                }

            } while (anyNewExplosion);
        }

        private static bool CanBeTargeted(PieceState target)
        {
            if (target.Mask == MaskType.Rat && !target.HasDealtDamage)
                return false;

            return true;
        }

        private static void PerformMoves(GameState state, GameState next, Dictionary<int, int> moveIntents)
        {
            foreach (var kv in moveIntents)
            {
                int pieceId = kv.Key;
                int targetCellId = kv.Value;

                if (!next.Pieces.TryGetValue(pieceId, out var piece))
                    continue;

                if (piece.Mask == MaskType.Carpenter)
                {
                    SpawnBarricade(next, piece);
                }

                piece.CellId = targetCellId;
                piece.LastActionTurn = state.TurnIndex;
            }
        }

        private static int barricadeIdCounter = -1;

        private static void SpawnBarricade(GameState state, PieceState carpenter)
        {
            var barricade = new PieceState
            {
                PieceId = new PieceId(barricadeIdCounter--),
                TeamId = carpenter.TeamId,
                CellId = carpenter.CellId,
                Mask = MaskType.Barricade,
                Health = 1,
                MaxHealth = 1,
                LastActionTurn = 0,
                HasDealtDamage = false
            };

            state.Pieces[barricade.PieceId.Value] = barricade;
        }


        private static int CalculateDamage(PieceState attacker, GameState state)
        {
            int damage = 1;

            if (attacker.Mask == MaskType.SeaWolf)
                damage = 2;

            if (attacker.Mask == MaskType.Parrot)
                damage = 0;

            // Ауры
            foreach (var ally in state.Pieces.Values)
            {
                if (!ally.IsAlive) continue;
                if (ally.TeamId != attacker.TeamId) continue;
                if (ally.Mask != MaskType.Cook) continue;

                if (state.Board.IsDirectConnected(ally.CellId, attacker.CellId) ||
                    state.Board.IsDiagonallyConnected(ally.CellId, attacker.CellId))
                {
                    damage += 1;
                }
            }

            return damage;
        }

        private static void CollectMoveIntent(
            GameState state,
            MoveOrder order,
            Dictionary<int, int> moveIntents
        )
        {
            if (order.Type != OrderType.Move)
                return;

            if (!state.Pieces.TryGetValue(order.PieceId, out var piece))
                return;

            if (piece.TeamId != order.TeamId)
                return;

            var maskDef = MaskDatabase.All[piece.Mask];
            if (!maskDef.MovementRule.CanMove(piece, state, order.TargetCellId))
                return;

            moveIntents[order.PieceId] = order.TargetCellId;
        }


        private static void ApplyEquipMask(
            GameState state,
            MoveOrder order
        )
        {
            if (order.Type != OrderType.EquipMask)
            {
                Debug.Log($"ApplyEquipMask called with invalid order type: {order.Type}. Expected EquipMask.");
                return;
            }

            var piece = state.Pieces[order.PieceId];

            if (piece.TeamId != order.TeamId)
            {
                Debug.Log($"Piece ID {order.PieceId} does not belong to team ID {order.TeamId}.");
                return;
            }

            var inventory = state.Inventories[order.TeamId];

            if (piece.Mask != MaskType.None)
            {
                Debug.Log($"Cannot equip mask to piece ID {order.PieceId} as it already has a mask: {piece.Mask}.");
                return;
            }

            if (!inventory.Counts.TryGetValue(order.Mask, out var count) || count <= 0)
            {
                Debug.Log($"Inventory for team ID {order.TeamId} does not have any masks of type {order.Mask}.");
                return;
            }

            piece.Mask = order.Mask;
            inventory.Counts[order.Mask]--;

            piece.LastActionTurn = state.TurnIndex;

            ApplyMaskStats(piece);
            Debug.Log($"Equipped mask {order.Mask} to piece ID {order.PieceId} for team ID {order.TeamId}. Remaining count: {inventory.Counts[order.Mask]}.");
        }

        private static void ApplyMaskStats(PieceState piece)
        {
            var newMaxHealth = MaskDatabase.All[piece.Mask].MaxHealth;

            piece.MaxHealth = newMaxHealth;
            piece.Health = Math.Min(piece.Health + newMaxHealth - 1, newMaxHealth);
        }

        private sealed class PlannedAttack
        {
            public PieceState Attacker;
            public PieceState Target;
            public int Damage;
        }
    }
}
