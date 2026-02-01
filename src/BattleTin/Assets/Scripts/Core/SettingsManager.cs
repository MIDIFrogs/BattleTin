using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public string PlayerNickname { get; set; }
    public float SoundVolume { get; set; }
    public float MusicVolume { get;  set; }
    public int ResolutionIndex { get; set; }
    public int WindowModeIndex { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSettings()
    {
        PlayerNickname = PlayerPrefs.GetString("Nickname", "Player");
        SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        ResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        WindowModeIndex = PlayerPrefs.GetInt("WindowModeIndex", 0);
    }

    public void SaveSettings(string nickname, float soundVol, float musicVol, int resIndex, int windowIndex)
    {
        PlayerNickname = nickname;
        SoundVolume = soundVol;
        MusicVolume = musicVol;
        ResolutionIndex = resIndex;
        WindowModeIndex = windowIndex;

        PlayerPrefs.SetString("Nickname", nickname);
        PlayerPrefs.SetFloat("SoundVolume", soundVol);
        PlayerPrefs.SetFloat("MusicVolume", musicVol);
        PlayerPrefs.SetInt("ResolutionIndex", resIndex);
        PlayerPrefs.SetInt("WindowModeIndex", windowIndex);
        PlayerPrefs.Save();
    }
}