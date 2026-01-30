using Unity.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Netcode
{
    public class NetBootstrap : MonoBehaviour
    {

        void Start()
        {
            if (IsHostBuild())
                NetworkManager.Singleton.StartHost();
            else
                NetworkManager.Singleton.StartClient();
        }

        bool IsHostBuild()
        {
            // временно, для тестов
            return true;
        }
    }
}
