using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AudioButtonAutoAssigner : MonoBehaviour
{
    [SerializeField] private BattleAudioManager battleAudioManager;
    public void PlayClickSound()
    {
        battleAudioManager.Click();
    }
}