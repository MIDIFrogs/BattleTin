using System;
using MIDIFrogs.BattleTin.Gameplay.Orders;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using Unity.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Netcode
{
    public class TurnSyncManager : NetworkBehaviour
    {
        public MoveOrder? RemoteOrder { get; private set; }

        public event Action<MoveOrder> OnRemoteConfirmed = delegate {};

        public void SendOrder(MoveOrder order)
        {
            SubmitOrderServerRpc(order);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void SubmitOrderServerRpc(ForceNetworkSerializeByMemcpy<MoveOrder> order)
        {
            ReceiveOrderClientRpc(order);
        }

        [ClientRpc]
        void ReceiveOrderClientRpc(ForceNetworkSerializeByMemcpy<MoveOrder> order)
        {
            if (order.Value.TeamId == MatchmakingManager.Instance.LocalTeamId)
                return;

            RemoteOrder = order;
            OnRemoteConfirmed(order);
        }
    }
}
