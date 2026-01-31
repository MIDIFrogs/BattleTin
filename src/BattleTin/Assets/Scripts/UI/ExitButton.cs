using Cysharp.Threading.Tasks;
using MIDIFrogs.BattleTin.Netcode.Assets.Scripts.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI
{
    internal class ExitButton : MonoBehaviour
    {
        public void OnClick()
        {
            MatchmakingManager.Instance.LeaveMatchAsync().Forget();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
