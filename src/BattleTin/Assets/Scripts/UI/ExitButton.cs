using UnityEngine;
using UnityEngine.SceneManagement;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI
{
    internal class ExitButton : MonoBehaviour
    {
        public void OnClick()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
