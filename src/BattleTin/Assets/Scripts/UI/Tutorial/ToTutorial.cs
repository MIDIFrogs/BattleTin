using UnityEngine;
using UnityEngine.SceneManagement;

namespace MIDIFrogs.BattleTin.UI.Assets.Scripts.UI.Tutorial
{
    internal class ToTutorial : MonoBehaviour
    {
        public void Move()
        {
            SceneManager.LoadScene("Tutorial");
        }
    }
}
