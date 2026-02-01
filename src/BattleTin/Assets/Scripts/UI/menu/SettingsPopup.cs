using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [Header("Resolution Buttons")]
    [SerializeField] private List<Button> resolutionButtons;
    [SerializeField] private Sprite resolutionNormalSprite;
    [SerializeField] private Sprite resolutionSelectedSprite;

    [Header("Window Mode Buttons")]
    [SerializeField] private List<Button> windowModeButtons;
    [SerializeField] private Sprite windowModeNormalSprite;
    [SerializeField] private Sprite windowModeSelectedSprite;

    [Header("Apply Button")]
    [SerializeField] private Button applyButton;

    private int selectedResolutionIndex = 1;
    private int selectedWindowModeIndex = 0;

    private string currentNickname = "Player";
    private float currentSoundVolume = 1f;
    private float currentMusicVolume = 0.8f;
    private bool settingsChanged = false;

    private void Start()
    {
        InitializeUI();
        LoadSettings();
        UpdateUI();
    }

    private void InitializeUI()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
            SetupButtonHover(closeButton);
        }

        if (nicknameInput != null)
        {
            nicknameInput.onValueChanged.AddListener(OnNicknameChanged);
            SetupInputFieldHover(nicknameInput);
        }

        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            SetupSliderHover(soundVolumeSlider);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            SetupSliderHover(musicVolumeSlider);
        }

        for (int i = 0; i < resolutionButtons.Count; i++)
        {
            int index = i;
            resolutionButtons[i].onClick.AddListener(() => OnResolutionSelected(index));
            SetupButtonHover(resolutionButtons[i]);
        }

        for (int i = 0; i < windowModeButtons.Count; i++)
        {
            int index = i;
            windowModeButtons[i].onClick.AddListener(() => OnWindowModeSelected(index));
            SetupButtonHover(windowModeButtons[i]);
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplySettings);
            applyButton.interactable = false;
            SetupButtonHover(applyButton);
        }
    }

    private void SetupButtonHover(Button button)
    {
        var hoverHandler = button.gameObject.AddComponent<ButtonHoverHandler>();
        hoverHandler.Setup(button);
    }

    private void SetupInputFieldHover(TMP_InputField inputField)
    {
        var hoverHandler = inputField.gameObject.AddComponent<InputFieldHoverHandler>();
        hoverHandler.Setup(inputField);
    }

    private void SetupSliderHover(Slider slider)
    {
        var hoverHandler = slider.gameObject.AddComponent<SliderHoverHandler>();
        hoverHandler.Setup(slider);
    }

    private void LoadSettings()
    {
        currentNickname = PlayerPrefs.GetString("Nickname", "Player");
        currentSoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        currentMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);

        selectedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        selectedWindowModeIndex = PlayerPrefs.GetInt("WindowModeIndex", 0);
    }

    private void UpdateUI()
    {
        if (nicknameInput != null)
            nicknameInput.text = currentNickname;

        if (soundVolumeSlider != null)
            soundVolumeSlider.value = currentSoundVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = currentMusicVolume;

        UpdateButtonSprites();
    }

    private void UpdateButtonSprites()
    {
        // Обновляем спрайты кнопок разрешения
        for (int i = 0; i < resolutionButtons.Count; i++)
        {
            if (resolutionButtons[i] != null)
            {
                var image = resolutionButtons[i].GetComponent<Image>();
                var text = resolutionButtons[i].GetComponentInChildren<TMP_Text>();

                if (image != null)
                {
                    bool isSelected = (i == selectedResolutionIndex);
                    image.sprite = isSelected ? resolutionSelectedSprite : resolutionNormalSprite;
                    image.color = isSelected ? Color.white : new Color(1, 1, 1, 0.7f);

                    // Меняем цвет текста на чёрный для выбранной кнопки
                    if (text != null)
                    {
                        text.color = isSelected ? Color.black : Color.white;
                    }
                }
            }
        }

        // Обновляем спрайты кнопок режима окна
        for (int i = 0; i < windowModeButtons.Count; i++)
        {
            if (windowModeButtons[i] != null)
            {
                var image = windowModeButtons[i].GetComponent<Image>();
                var text = windowModeButtons[i].GetComponentInChildren<TMP_Text>();

                if (image != null)
                {
                    bool isSelected = (i == selectedWindowModeIndex);
                    image.sprite = isSelected ? windowModeSelectedSprite : windowModeNormalSprite;
                    image.color = isSelected ? Color.white : new Color(1, 1, 1, 0.7f);

                }
            }
        }
    }

    private void OnNicknameChanged(string newNickname)
    {
        if (newNickname != currentNickname)
        {
            settingsChanged = true;
            if (applyButton != null)
                applyButton.interactable = true;
        }
    }

    private void OnSoundVolumeChanged(float value)
    {
        // Сохраняем звук сразу
        currentSoundVolume = value;
        PlayerPrefs.SetFloat("SoundVolume", currentSoundVolume);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateSoundVolume(currentSoundVolume);
        }

        // Для остальных настроек оставляем Apply
        if (Mathf.Abs(value - PlayerPrefs.GetFloat("SoundVolume", 1f)) > 0.01f)
        {
            settingsChanged = true;
            if (applyButton != null)
                applyButton.interactable = true;
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        // Сохраняем музыку сразу
        currentMusicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", currentMusicVolume);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateMusicVolume(currentMusicVolume);
        }

        // Для остальных настроек оставляем Apply
        if (Mathf.Abs(value - PlayerPrefs.GetFloat("MusicVolume", 0.8f)) > 0.01f)
        {
            settingsChanged = true;
            if (applyButton != null)
                applyButton.interactable = true;
        }
    }

    private void OnResolutionSelected(int index)
    {
        if (index != selectedResolutionIndex)
        {
            selectedResolutionIndex = index;
            UpdateButtonSprites();
            settingsChanged = true;

            if (applyButton != null)
                applyButton.interactable = true;
        }
    }

    private void OnWindowModeSelected(int index)
    {
        if (index != selectedWindowModeIndex)
        {
            selectedWindowModeIndex = index;
            UpdateButtonSprites();
            settingsChanged = true;

            if (applyButton != null)
                applyButton.interactable = true;
        }
    }

    public void ApplySettings()
    {
        if (!settingsChanged) return;

        currentNickname = nicknameInput.text;

        PlayerPrefs.SetString("Nickname", currentNickname);
        PlayerPrefs.SetFloat("SoundVolume", currentSoundVolume);
        PlayerPrefs.SetFloat("MusicVolume", currentMusicVolume);
        PlayerPrefs.SetInt("ResolutionIndex", selectedResolutionIndex);
        PlayerPrefs.SetInt("WindowModeIndex", selectedWindowModeIndex);
        PlayerPrefs.Save();

        ApplyResolution(selectedResolutionIndex);

        settingsChanged = false;
        if (applyButton != null)
            applyButton.interactable = false;

        Debug.Log("Settings applied!");
    }

    private void ApplyResolution(int resolutionIndex)
    {
        int width = 1920;
        int height = 1080;

        switch (resolutionIndex)
        {
            case 0: width = 1280; height = 720; break;
            case 1: width = 1920; height = 1080; break;
            case 2: width = 2560; height = 1440; break;
            case 3: width = 3840; height = 2160; break;
        }

        FullScreenMode mode = FullScreenMode.FullScreenWindow;
        switch (selectedWindowModeIndex)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: mode = FullScreenMode.FullScreenWindow; break;
            case 2: mode = FullScreenMode.Windowed; break;
        }
        Screen.SetResolution(width, height, mode);
    }

    private void ClosePopup()
    {
        // Если есть несохраненные изменения, спросить пользователя
        if (settingsChanged)
        {
            // Здесь можно добавить диалоговое окно с подтверждением
            ApplySettings(); // Или просто сохранить
        }

        if (MenuManager.Instance != null)
            MenuManager.Instance.ClosePopup();
    }

    private void OnDestroy()
    {
        // Очистка событий
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        if (nicknameInput != null)
            nicknameInput.onValueChanged.RemoveAllListeners();

        if (soundVolumeSlider != null)
            soundVolumeSlider.onValueChanged.RemoveAllListeners();

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveAllListeners();

        foreach (var button in resolutionButtons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }

        foreach (var button in windowModeButtons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }

        if (applyButton != null)
            applyButton.onClick.RemoveAllListeners();
    }
}