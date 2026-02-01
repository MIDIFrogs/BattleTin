using UnityEngine;
using UnityEngine.UI;
using TMPro; // Для TextMeshPro (опционально)

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements - Слайдеры")]
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [Header("Настройки")]
    [SerializeField] private bool updateInRealTime = true;

    private void Start()
    {
        InitializeSliders();
        LoadCurrentSettings();

        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
    }

    private void InitializeSliders()
    {
        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.minValue = 0f;
            soundVolumeSlider.maxValue = 1f;
            soundVolumeSlider.wholeNumbers = false;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.wholeNumbers = false;
        }
    }

    private void LoadCurrentSettings()
    {
        float soundVolume = SettingsManager.Instance.SoundVolume;
        float musicVolume = SettingsManager.Instance.MusicVolume;

        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.value = soundVolume;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
        }

        if (updateInRealTime)
        {
        }
    }

    private void OnSoundVolumeChanged(float value)
    {
        SettingsManager.Instance.SoundVolume = value;
    }

    private void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.MusicVolume = value;
    }



    public void SaveAllSettings()
    {
        // Сохраняем все настройки через SettingsManager
        SettingsManager.Instance.SaveSettings(
            SettingsManager.Instance.PlayerNickname,
            SettingsManager.Instance.SoundVolume,
            SettingsManager.Instance.MusicVolume,
            SettingsManager.Instance.ResolutionIndex,
            SettingsManager.Instance.WindowModeIndex
        );
    }

   
    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении
        if (soundVolumeSlider != null)
            soundVolumeSlider.onValueChanged.RemoveListener(OnSoundVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }
}