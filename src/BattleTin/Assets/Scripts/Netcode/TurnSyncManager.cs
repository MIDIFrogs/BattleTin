using MIDIFrogs.BattleTin.Gameplay.Orders;
using Unity.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Netcode
{
    public class TurnSyncManager : NetworkBehaviour
    {
        private MoveOrder? localOrder;
        private MoveOrder? remoteOrder;

        public void SetLocalOrder(MoveOrder order)
        {
            localOrder = order;
        }

        public void ConfirmTurn()
        {
            if (!localOrder.HasValue)
                return;

            SubmitOrderServerRpc(localOrder.Value);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void SubmitOrderServerRpc(ForceNetworkSerializeByMemcpy<MoveOrder> order)
        {
            ReceiveOrderClientRpc(order);
        }

        [ClientRpc]
        void ReceiveOrderClientRpc(ForceNetworkSerializeByMemcpy<MoveOrder> order)
        {
            if (order.Value.PlayerId == GetLocalPlayerId())
                return;

            remoteOrder = order;
            TryResolveTurn();
        }

        void TryResolveTurn()
        {
            if (!localOrder.HasValue || !remoteOrder.HasValue)
                return;

            // вызов симул€ции
            Debug.Log("Both orders received, resolving turn");

            localOrder = null;
            remoteOrder = null;
        }

        int GetLocalPlayerId()
        {
            return (int)NetworkManager.Singleton.LocalClientId;
        }
    }
}
