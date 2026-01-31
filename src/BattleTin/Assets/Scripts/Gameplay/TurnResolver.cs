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

            // === 3. Проводим симуляцию боя ===
            SimulateBattle(next);

            return next;
        }

        private static void SimulateBattle(GameState state)
        {
            var aliveUnits = state.Pieces.Values.Where(u => u.IsAlive).ToList();

            Debug.Log($"Alive units number: {aliveUnits.Count}");

            foreach (var unit in aliveUnits)
            {
                Debug.Log($"Unit #{unit.PieceId.Value}: HP={unit.Hp}, TeamId={unit.TeamId}, CellId={unit.CellId.Value}");
            }

            var virtualHp = aliveUnits.ToDictionary(u => u.PieceId, u => u.Hp);
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

                int damage = CalculateDamage(attacker, target, state);
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
                attack.Target.Hp -= attack.Damage;

                Debug.Log($"Target #{attack.Target.PieceId.Value} final HP after applying damage: {attack.Target.Hp}");
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

        private static int CalculateDamage(PieceState attacker, PieceState target, GameState state)
        {
            // TODO:
            // - учесть маску атакующего
            // - бафы от союзников
            // - дебаффы цели

            return 1;
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

        private sealed class PlannedAttack
        {
            public PieceState Attacker;
            public PieceState Target;
            public int Damage;
        }
    }
}
