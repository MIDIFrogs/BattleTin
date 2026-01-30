using System.Collections.Generic;
using MIDIFrogs.BattleTin.Gameplay.Orders;

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
            foreach (var kv in moveIntents)
            {
                int pieceId = kv.Key;
                int targetCellId = kv.Value;

                if (!next.Pieces.TryGetValue(pieceId, out var piece))
                    continue;

                piece.CellId = targetCellId;
            }

            // === 3. Бои будут здесь позже ===

            return next;
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

            //// базовая валидация
            //if (piece.OwnerPlayerId != order.PlayerId)
            //    return;

            // последняя запись побеждает (на будущее, если будут сложные эффекты)
            moveIntents[order.PieceId] = order.TargetCellId;
        }
    }
}
