using UnityEngine;
using System.Collections.Generic;

public enum AudioType
{
    Sound,
    Music
}

public class BattleAudioManager : MonoBehaviour
{
    public static BattleAudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource themeBattleSecondAudioSource;
    [SerializeField] private AudioSource themeBattleThirdAudioSource;
    [SerializeField] private AudioSource matchResultAudioSource;
    [SerializeField] private AudioSource battleAudioSource;
    [SerializeField] private AudioSource pickUpUnitAudioSource;
    [SerializeField] private AudioSource turnsAudioSource;
    [SerializeField] private AudioSource walkAudioSource;
    [SerializeField] private AudioSource stateBattleAudioSource;
    [SerializeField] private AudioSource clickButtonAudioSource;

    [Header("Battle Theme Clips")]
    [SerializeField] private AudioClip battleThemeSecond;
    [SerializeField] private AudioClip battleThemeThird;

    [Header("Match Result Clips")]
    [SerializeField] private AudioClip resultWin;
    [SerializeField] private AudioClip resultLose;

    [Header("Battle Audio Clips")]
    [SerializeField] private AudioClip battleSound;

    [Header("Pick Up Unit Clips")]
    [SerializeField] private AudioClip captainPickUp;
    [SerializeField] private AudioClip carpenterPickUp;
    [SerializeField] private AudioClip cookPickUp;
    [SerializeField] private AudioClip corsairPickUp;
    [SerializeField] private AudioClip seaWolfPickUp;
    [SerializeField] private AudioClip parrotPickUp;
    [SerializeField] private AudioClip ratPickUp;
    [SerializeField] private AudioClip salagaPickUp;

    [Header("Turns Audio Clips")]
    [SerializeField] private AudioClip endTurnSound;
    [SerializeField] private AudioClip switchTurnSound;

    [Header("Walk Audio Clips")]
    [SerializeField] private AudioClip captainWalk;
    [SerializeField] private AudioClip carpenterWalk;
    [SerializeField] private AudioClip cookWalk;
    [SerializeField] private AudioClip corsairWalk;
    [SerializeField] private AudioClip seaWolfWalk;
    [SerializeField] private AudioClip parrotWalk;
    [SerializeField] private AudioClip ratWalk;
    [SerializeField] private AudioClip salagaWalk;

    [Header("State Battle Audio Clips")]
    [SerializeField] private AudioClip battleLosingSound;
    [SerializeField] private AudioClip battleWinningSound;

    [Header("Audio Settings")]
    [SerializeField] private float defaultSoundVolume = 1f;
    [SerializeField] private float defaultMusicVolume = 0.8f;

    [Header("Audio Cooldowns")]
    [SerializeField] private float pickUpCooldown = 0.1f;
    [SerializeField] private float walkCooldown = 0.2f;

    // Громкости из настроек (будут получаться из SettingsManager)
    private float soundVolume = 1f;
    private float musicVolume = 0.8f;
    private bool isInitialized = false;

    private Dictionary<string, AudioClip> pickUpSounds;
    private Dictionary<string, AudioClip> walkSounds;

    private float lastPickUpTime = 0f;
    private float lastWalkTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionaries();

            // Загружаем настройки при инициализации
            LoadAudioSettings();
            isInitialized = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyAudioVolumes();
    }

    private void LoadAudioSettings()
    {
        // ВАЖНО: Если SettingsManager существует, используем его настройки
        if (SettingsManager.Instance != null)
        {
            // Используем SettingsManager как основной источник настроек
            soundVolume = SettingsManager.Instance.SoundVolume;
            musicVolume = SettingsManager.Instance.MusicVolume;
        }
        else
        {
            // Fallback на PlayerPrefs если SettingsManager нет
            soundVolume = PlayerPrefs.GetFloat("SoundVolume", defaultSoundVolume);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);

            Debug.LogWarning("SettingsManager не найден. Использую PlayerPrefs для настроек аудио.");
        }
    }

    private void InitializeDictionaries()
    {
        pickUpSounds = new Dictionary<string, AudioClip>
        {
            { "captain", captainPickUp },
            { "carpenter", carpenterPickUp },
            { "cook", cookPickUp },
            { "corsair", corsairPickUp },
            { "seawolf", seaWolfPickUp },
            { "parrot", parrotPickUp },
            { "rat", ratPickUp },
            { "salaga", salagaPickUp }
        };

        walkSounds = new Dictionary<string, AudioClip>
        {
            { "captain", captainWalk },
            { "carpenter", carpenterWalk },
            { "cook", cookWalk },
            { "corsair", corsairWalk },
            { "seawolf", seaWolfWalk },
            { "parrot", parrotWalk },
            { "rat", ratWalk },
            { "salaga", salagaWalk }
        };
    }

    private void ApplyAudioVolumes()
    {
        if (!isInitialized) return;

        // Применяем громкости к каждому AudioSource
        // Источники музыки
        if (themeBattleSecondAudioSource != null)
            themeBattleSecondAudioSource.volume = musicVolume;

        if (themeBattleThirdAudioSource != null)
            themeBattleThirdAudioSource.volume = musicVolume;

        // Источники звуков (SFX)
        if (matchResultAudioSource != null)
            matchResultAudioSource.volume = soundVolume;

        if (battleAudioSource != null)
            battleAudioSource.volume = soundVolume;

        if (pickUpUnitAudioSource != null)
            pickUpUnitAudioSource.volume = soundVolume;

        if (turnsAudioSource != null)
            turnsAudioSource.volume = soundVolume;

        if (walkAudioSource != null)
            walkAudioSource.volume = soundVolume;

        if (stateBattleAudioSource != null)
            stateBattleAudioSource.volume = soundVolume;

        if (clickButtonAudioSource != null)
            clickButtonAudioSource.volume = soundVolume;
    }

    // === МЕТОД ДЛЯ ВОСПРОИЗВЕДЕНИЯ С УЧЕТОМ НАСТРОЕК ===

    /// <summary>
    /// Основной метод для воспроизведения звуков с учетом настроек громкости
    /// </summary>
    /// <param name="type">Тип звука (Sound или Music)</param>
    /// <param name="audioSource">AudioSource для воспроизведения</param>
    /// <param name="clip">Аудиоклип</param>
    /// <param name="volumeMultiplier">Множитель громкости (от 0 до 1)</param>
    /// <param name="loop">Зациклить ли звук</param>
    public void PlayWithSetting(AudioType type, AudioSource audioSource, AudioClip clip, float volumeMultiplier = 1f, bool loop = false)
    {
        if (!isInitialized || audioSource == null || clip == null) return;

        // Определяем базовую громкость в зависимости от типа
        float baseVolume = (type == AudioType.Music) ? musicVolume : soundVolume;

        // Вычисляем финальную громкость
        float finalVolume = baseVolume * Mathf.Clamp01(volumeMultiplier);

        if (finalVolume < 0.01f) return; // Если громкость практически 0, не воспроизводим

        if (loop)
        {
            // Для зацикленных звуков
            audioSource.clip = clip;
            audioSource.volume = finalVolume;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            // Для одноразовых звуков
            audioSource.PlayOneShot(clip, finalVolume);
        }
    }

    // === АДАПТИРОВАННЫЕ МЕТОДЫ ДЛЯ THEME BATTLE ===

    public void PlayThemeBattleSecond()
    {
        if (!isInitialized || themeBattleSecondAudioSource == null || battleThemeSecond == null) return;

        // Используем PlayWithSetting для музыки
        PlayWithSetting(AudioType.Music, themeBattleSecondAudioSource, battleThemeSecond, 1f, true);
    }

    public void PlayThemeBattleThird()
    {
        if (!isInitialized || themeBattleThirdAudioSource == null || battleThemeThird == null) return;

        // Используем PlayWithSetting для музыки
        PlayWithSetting(AudioType.Music, themeBattleThirdAudioSource, battleThemeThird, 1f, true);
    }

    public void StopThemeBattleSecond()
    {
        if (themeBattleSecondAudioSource != null && themeBattleSecondAudioSource.isPlaying)
        {
            themeBattleSecondAudioSource.Stop();
        }
    }

    public void StopThemeBattleThird()
    {
        if (themeBattleThirdAudioSource != null && themeBattleThirdAudioSource.isPlaying)
        {
            themeBattleThirdAudioSource.Stop();
        }
    }

    public void Click()
    {
        if (clickButtonAudioSource != null)
        {
            // Используем PlayWithSetting для звука кнопки
            // Если нет клипа, используем стандартное воспроизведение
            if (clickButtonAudioSource.clip != null)
            {
                PlayWithSetting(AudioType.Sound, clickButtonAudioSource, clickButtonAudioSource.clip);
            }
            else
            {
                clickButtonAudioSource.Play();
            }
        }
    }

    // === АДАПТИРОВАННЫЕ МЕТОДЫ ДЛЯ РЕЗУЛЬТАТОВ МАТЧА ===

    public void PlayResultWin()
    {
        PlayOneShotWithSettings(matchResultAudioSource, resultWin);
    }

    public void PlayResultLose()
    {
        PlayOneShotWithSettings(matchResultAudioSource, resultLose);
    }

    // === АДАПТИРОВАННЫЕ МЕТОДЫ ДЛЯ БОЕВЫХ ЗВУКОВ ===

    public void PlayBattleSound()
    {
        PlayOneShotWithSettings(battleAudioSource, battleSound);
    }

    // === АДАПТИРОВАННЫЕ МЕТОДЫ ДЛЯ ПОДНЯТИЯ ЮНИТОВ ===

    public void PlayCaptainPickUp() => PlayPickUpSound("captain");
    public void PlayCarpenterPickUp() => PlayPickUpSound("carpenter");
    public void PlayCookPickUp() => PlayPickUpSound("cook");
    public void PlayCorsairPickUp() => PlayPickUpSound("corsair");
    public void PlaySeaWolfPickUp() => PlayPickUpSound("seawolf");
    public void PlayParrotPickUp() => PlayPickUpSound("parrot");
    public void PlayRatPickUp() => PlayPickUpSound("rat");
    public void PlaySalagaPickUp() => PlayPickUpSound("salaga");

    private void PlayPickUpSound(string unitType)
    {
        if (!isInitialized || pickUpUnitAudioSource == null) return;

        // Защита от спама
        if (Time.time - lastPickUpTime < pickUpCooldown) return;

        if (pickUpSounds.TryGetValue(unitType.ToLower(), out AudioClip clip) && clip != null)
        {
            // Используем PlayWithSetting для звуков поднятия
            PlayWithSetting(AudioType.Sound, pickUpUnitAudioSource, clip);
            lastPickUpTime = Time.time;
        }
        else
        {
            Debug.LogWarning($"No pickup sound found for unit type: {unitType}");
        }
    }

    // === АДАПТИРОВАННЫЕ МЕТОДЫ ДЛЯ ХОДОВ ===

    public void PlayEndTurnSound()
    {
        PlayOneShotWithSettings(turnsAudioSource, endTurnSound);
    }

    public void PlaySwitchTurnSound()
    {
        PlayOneShotWithSettings(turnsAudioSource, switchTurnSound);
    }

    // === АДАПТИРОВАННЫЕ МЕТОДЫ ДЛЯ ЗВУКОВ ХОДЬБЫ ===

    public void PlayCaptainWalk() => PlayWalkSound("captain");
    public void PlayCarpenterWalk() => PlayWalkSound("carpenter");
    public void PlayCookWalk() => PlayWalkSound("cook");
    public void PlayCorsairWalk() => PlayWalkSound("corsair");
    public void PlaySeaWolfWalk() => PlayWalkSound("seawolf");
    public void PlayParrotWalk() => PlayWalkSound("parrot");
    public void PlayRatWalk() => PlayWalkSound("rat");
    public void PlaySalagaWalk() => PlayWalkSound("salaga");

    private void PlayWalkSound(string unitType)
    {
        if (!isInitialized || walkAudioSource == null) return;

        // Защита от спама
        if (Time.time - lastWalkTime < walkCooldown) return;

        if (walkSounds.TryGetValue(unitType.ToLower(), out AudioClip clip) && clip != null)
        {
            // Используем PlayWithSetting для звуков ходьбы
            PlayWithSetting(AudioType.Sound, walkAudioSource, clip);
            lastWalkTime = Time.time;
        }
        else
        {
            Debug.LogWarning($"No walk sound found for unit type: {unitType}");
        }
    }

    // === АДАПТИРОВАННЫЕ МЕТОДЫ ДЛЯ СОСТОЯНИЯ БИТВЫ ===

    public void PlayBattleLosingSoundStart()
    {
        if (!isInitialized || stateBattleAudioSource == null || battleLosingSound == null) return;

        if (!stateBattleAudioSource.isPlaying)
        {
            // Используем PlayWithSetting для музыки состояния боя
            PlayWithSetting(AudioType.Music, stateBattleAudioSource, battleLosingSound, 1f, true);
        }
    }

    public void PlayBattleWinningSoundStart()
    {
        if (!isInitialized || stateBattleAudioSource == null || battleWinningSound == null) return;

        if (!stateBattleAudioSource.isPlaying)
        {
            // Используем PlayWithSetting для музыки состояния боя
            PlayWithSetting(AudioType.Music, stateBattleAudioSource, battleWinningSound, 1f, true);
        }
    }

    public void PlayBattleLosingSoundEnd()
    {
        if (stateBattleAudioSource != null &&
            stateBattleAudioSource.isPlaying &&
            stateBattleAudioSource.clip == battleLosingSound)
        {
            stateBattleAudioSource.Stop();
        }
    }

    public void PlayBattleWinningSoundEnd()
    {
        if (stateBattleAudioSource != null &&
            stateBattleAudioSource.isPlaying &&
            stateBattleAudioSource.clip == battleWinningSound)
        {
            stateBattleAudioSource.Stop();
        }
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (АДАПТИРОВАННЫЕ) ===

    private void PlayOneShotWithSettings(AudioSource audioSource, AudioClip clip)
    {
        if (!isInitialized || audioSource == null || clip == null) return;

        // Используем PlayWithSetting вместо прямого PlayOneShot
        PlayWithSetting(AudioType.Sound, audioSource, clip);
    }

    // === ОБНОВЛЕННЫЕ МЕТОДЫ ДЛЯ УПРАВЛЕНИЯ ГРОМКОСТЬЮ ===

    public void UpdateSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        ApplyAudioVolumes();

        // Сохраняем в оба места для совместимости
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);

        // Если SettingsManager существует, обновляем и там
        if (SettingsManager.Instance != null)
        {
            // Нужно сохранить все настройки, поэтому получаем текущие значения
            SettingsManager.Instance.SaveSettings(
                SettingsManager.Instance.PlayerNickname,
                soundVolume,
                SettingsManager.Instance.MusicVolume,
                SettingsManager.Instance.ResolutionIndex,
                SettingsManager.Instance.WindowModeIndex
            );
        }
    }

    public void UpdateMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyAudioVolumes();

        // Сохраняем в оба места для совместимости
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        // Если SettingsManager существует, обновляем и там
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SaveSettings(
                SettingsManager.Instance.PlayerNickname,
                SettingsManager.Instance.SoundVolume,
                musicVolume,
                SettingsManager.Instance.ResolutionIndex,
                SettingsManager.Instance.WindowModeIndex
            );
        }

        // Если музыка остановилась, но громкость > 0 - перезапускаем
        if (themeBattleSecondAudioSource != null &&
            !themeBattleSecondAudioSource.isPlaying &&
            themeBattleSecondAudioSource.clip != null &&
            musicVolume > 0.01f)
        {
            themeBattleSecondAudioSource.Play();
        }

        if (themeBattleThirdAudioSource != null &&
            !themeBattleThirdAudioSource.isPlaying &&
            themeBattleThirdAudioSource.clip != null &&
            musicVolume > 0.01f)
        {
            themeBattleThirdAudioSource.Play();
        }
    }

    /// <summary>
    /// Синхронизирует настройки громкости с SettingsManager
    /// Вызывайте этот метод при старте сцены или при изменении настроек
    /// </summary>
    public void SyncWithSettingsManager()
    {
        if (SettingsManager.Instance != null)
        {
            soundVolume = SettingsManager.Instance.SoundVolume;
            musicVolume = SettingsManager.Instance.MusicVolume;
            ApplyAudioVolumes();
        }
        else
        {
            Debug.LogWarning("SettingsManager не найден при синхронизации");
        }
    }

    public float GetSoundVolume() => soundVolume;
    public float GetMusicVolume() => musicVolume;

    // === МЕТОДЫ ДЛЯ ОСТАНОВКИ ВСЕХ ЗВУКОВ (БЕЗ ИЗМЕНЕНИЙ) ===

    public void StopAllSounds()
    {
        if (themeBattleSecondAudioSource != null && themeBattleSecondAudioSource.isPlaying)
            themeBattleSecondAudioSource.Stop();

        if (themeBattleThirdAudioSource != null && themeBattleThirdAudioSource.isPlaying)
            themeBattleThirdAudioSource.Stop();

        if (matchResultAudioSource != null && matchResultAudioSource.isPlaying)
            matchResultAudioSource.Stop();

        if (battleAudioSource != null && battleAudioSource.isPlaying)
            battleAudioSource.Stop();

        if (pickUpUnitAudioSource != null && pickUpUnitAudioSource.isPlaying)
            pickUpUnitAudioSource.Stop();

        if (turnsAudioSource != null && turnsAudioSource.isPlaying)
            turnsAudioSource.Stop();

        if (walkAudioSource != null && walkAudioSource.isPlaying)
            walkAudioSource.Stop();

        if (stateBattleAudioSource != null && stateBattleAudioSource.isPlaying)
            stateBattleAudioSource.Stop();
    }

    public void PauseAllSounds()
    {
        if (themeBattleSecondAudioSource != null) themeBattleSecondAudioSource.Pause();
        if (themeBattleThirdAudioSource != null) themeBattleThirdAudioSource.Pause();
        if (matchResultAudioSource != null) matchResultAudioSource.Pause();
        if (battleAudioSource != null) battleAudioSource.Pause();
        if (pickUpUnitAudioSource != null) pickUpUnitAudioSource.Pause();
        if (turnsAudioSource != null) turnsAudioSource.Pause();
        if (walkAudioSource != null) walkAudioSource.Pause();
        if (stateBattleAudioSource != null) stateBattleAudioSource.Pause();
    }

    public void ResumeAllSounds()
    {
        if (themeBattleSecondAudioSource != null && themeBattleSecondAudioSource.clip != null)
            themeBattleSecondAudioSource.UnPause();
        if (themeBattleThirdAudioSource != null && themeBattleThirdAudioSource.clip != null)
            themeBattleThirdAudioSource.UnPause();
        if (matchResultAudioSource != null && matchResultAudioSource.clip != null)
            matchResultAudioSource.UnPause();
        if (battleAudioSource != null && battleAudioSource.clip != null)
            battleAudioSource.UnPause();
        if (pickUpUnitAudioSource != null && pickUpUnitAudioSource.clip != null)
            pickUpUnitAudioSource.UnPause();
        if (turnsAudioSource != null && turnsAudioSource.clip != null)
            turnsAudioSource.UnPause();
        if (walkAudioSource != null && walkAudioSource.clip != null)
            walkAudioSource.UnPause();
        if (stateBattleAudioSource != null && stateBattleAudioSource.clip != null)
            stateBattleAudioSource.UnPause();
    }
}