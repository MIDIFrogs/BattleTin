using UnityEngine;
using UnityEngine.SceneManagement;

namespace MIDIFrogs.BattleTin.Core
{
    internal class BootstrapManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] dontDestroyOnLoad;

        private void Awake()
        {
            foreach (var item in dontDestroyOnLoad)
                DontDestroyOnLoad(item);
            SceneManager.LoadScene("Main Menu");
        }
    }
}
