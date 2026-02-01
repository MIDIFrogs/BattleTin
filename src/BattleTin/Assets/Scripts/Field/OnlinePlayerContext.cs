using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public class OnlinePlayerContext : MonoBehaviour, IPlayerContext
    {
        public int LocalTeamId => MatchmakingManager.Instance.LocalTeamId;
    }

}
