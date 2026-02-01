using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    public class OfflinePlayerContext : MonoBehaviour, IPlayerContext
    {
        [SerializeField] private int localTeamId = 0;
        public int LocalTeamId => localTeamId;
    }

}
