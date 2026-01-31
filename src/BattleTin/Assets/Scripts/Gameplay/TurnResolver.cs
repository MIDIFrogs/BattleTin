using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using UnityEditor;
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
                .OrderBy(u => u.LastActionTurn)
                .ThenBy(u => u.PieceId.Value)
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
                        (
                            t.CellId == attacker.CellId ||
                            state.Board.IsDirectConnected(attacker.CellId, t.CellId)
                        )
                    )
                    .OrderBy(t => t.LastActionTurn)
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
                attack.Target.Health -= attack.Damage;

                Debug.Log($"Target #{attack.Target.PieceId.Value} final HP after applying damage: {attack.Target.Health}");
            }

            var toRemove = state.Pieces.Values.Where(u => !u.IsAlive).ToList();

            foreach (var p in toRemove)
            {
                state.Pieces.Remove(p.PieceId.Value);
                Debug.Log($"Removing unit #{p.PieceId.Value} from the field.");
            }
        }

        private static void PerformMoves(GameState state, GameState next, Dictionary<int, int> moveIntents)
        {
            foreach (var kv in moveIntents)
            {
                int pieceId = kv.Key;
                int targetCellId = kv.Value;

                if (!next.Pieces.TryGetValue(pieceId, out var piece))
                    continue;

                piece.CellId = targetCellId;
                piece.LastActionTurn = state.TurnIndex;
            }
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

            // базовая валидация
            if (piece.TeamId != order.TeamId)
                return;

            // последняя запись побеждает (на будущее, если будут сложные эффекты)
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
            switch (piece.Mask)
            {
                case MaskType.SeaWolf:
                    piece.MaxHealth = 2;
                    piece.Health = Math.Min(piece.Health + 1, 2);
                    break;

                case MaskType.Cook:
                    piece.MaxHealth = 2;
                    piece.Health = Math.Min(piece.Health + 1, 2);
                    break;

                case MaskType.Rat:
                    piece.MaxHealth = 1;
                    break;

                case MaskType.Parrot:
                    piece.MaxHealth = 3;
                    piece.Health = Math.Min(piece.Health + 2, 3);
                    break;

                case MaskType.Cannoneer:
                    piece.MaxHealth = 1;
                    break;

                case MaskType.Carpenter:
                    piece.MaxHealth = 2;
                    piece.Health = Math.Min(piece.Health + 1, 2);
                    break;
            }
        }


        private sealed class PlannedAttack
        {
            public PieceState Attacker;
            public PieceState Target;
            public int Damage;
        }
    }
}
