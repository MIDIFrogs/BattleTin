using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIDIFrogs.BattleTin.Gameplay;
using MIDIFrogs.BattleTin.Gameplay.Board;
using MIDIFrogs.BattleTin.Gameplay.Pieces;
using Unity.Netcode;

namespace MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode
{
    [Serializable]
    public struct GameStateSnapshot : INetworkSerializable
    {
        public int TurnIndex;
        public bool GameOver;
        public int WinnerTeamId;

        public PieceSnapshot[] Pieces;
        public InventorySnapshot[] Inventories;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref TurnIndex);
            serializer.SerializeValue(ref GameOver);
            serializer.SerializeValue(ref WinnerTeamId);

            serializer.SerializeValue(ref Pieces);
            serializer.SerializeValue(ref Inventories);
        }
    }

    [Serializable]
    public struct PieceSnapshot : INetworkSerializable
    {
        public int PieceId;
        public int TeamId;
        public int CellId;
        public int Health;
        public MaskType Mask;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref PieceId);
            serializer.SerializeValue(ref TeamId);
            serializer.SerializeValue(ref CellId);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref Mask);
        }
    }

    [Serializable]
    public struct InventorySnapshot : INetworkSerializable
    {
        public int TeamId;
        public MaskEntry[] Masks;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref TeamId);
            serializer.SerializeValue(ref Masks);
        }
    }

    [Serializable]
    public struct MaskEntry : INetworkSerializable
    {
        public MaskType Mask;
        public int Count;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref Mask);
            serializer.SerializeValue(ref Count);
        }
    }

    public static class GameStateExtensions
    {
        public static GameStateSnapshot ToSnapshot(this GameState state)
        {
            return new GameStateSnapshot
            {
                TurnIndex = state.TurnIndex,
                GameOver = state.GameOver,
                WinnerTeamId = state.WinnerTeamId,

                Pieces = state.Pieces.Values.Select(p => new PieceSnapshot
                {
                    PieceId = p.PieceId.Value,
                    TeamId = p.TeamId,
                    CellId = p.CellId.Value,
                    Health = p.Health,
                    Mask = p.Mask
                }).ToArray(),

                Inventories = state.Inventories.Values.Select(inv => new InventorySnapshot
                {
                    TeamId = inv.TeamId,
                    Masks = inv.Counts.Select(m => new MaskEntry
                    {
                        Mask = m.Key,
                        Count = m.Value
                    }).ToArray()
                }).ToArray()
            };
        }

        public static GameState FromSnapshot(
            this GameStateSnapshot snap,
            BoardGraph board
        )
        {
            var state = new GameState(board)
            {
                TurnIndex = snap.TurnIndex,
                GameOver = snap.GameOver,
                WinnerTeamId = snap.WinnerTeamId
            };

            foreach (var p in snap.Pieces)
            {
                state.Pieces[p.PieceId] = new PieceState
                {
                    PieceId = new(p.PieceId),
                    TeamId = p.TeamId,
                    CellId = new(p.CellId),
                    Health = p.Health,
                    Mask = p.Mask
                };
            }

            foreach (var inv in snap.Inventories)
            {
                state.Inventories[inv.TeamId] = FromSnapshot(inv);
            }

            return state;
        }

        public static MaskInventory FromSnapshot(this InventorySnapshot snap)
        {
            return new()
            {
                Counts = snap.Masks.ToDictionary(x => x.Mask, y => y.Count),
                TeamId = snap.TeamId,
            };
        }
    }
}
