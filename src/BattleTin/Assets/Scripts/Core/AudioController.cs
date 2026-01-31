using UnityEngine;
using System.Collections.Generic;

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
            LoadAudioSettings();
            InitializeDictionaries();
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
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", defaultSoundVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
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
        if (themeBattleSecondAudioSource != null)
            themeBattleSecondAudioSource.volume = musicVolume;

        if (themeBattleThirdAudioSource != null)
            themeBattleThirdAudioSource.volume = musicVolume;

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
    }

    // === Ã≈“Œƒ€ ƒÀﬂ THEME BATTLE ===

    public void PlayThemeBattleSecond()
    {
        if (!isInitialized || themeBattleSecondAudioSource == null || battleThemeSecond == null) return;

        if (!themeBattleSecondAudioSource.isPlaying)
        {
            themeBattleSecondAudioSource.clip = battleThemeSecond;
            themeBattleSecondAudioSource.loop = true;
            themeBattleSecondAudioSource.Play();
        }
    }

    public void PlayThemeBattleThird()
    {
        if (!isInitialized || themeBattleThirdAudioSource == null || battleThemeThird == null) return;

        if (!themeBattleThirdAudioSource.isPlaying)
        {
            themeBattleThirdAudioSource.clip = battleThemeThird;
            themeBattleThirdAudioSource.loop = true;
            themeBattleThirdAudioSource.Play();
        }
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

    // === Ã≈“Œƒ€ ƒÀﬂ –≈«”À‹“¿“Œ¬ Ã¿“◊¿ ===

    public void PlayResultWin()
    {
        PlayOneShot(matchResultAudioSource, resultWin);
    }

    public void PlayResultLose()
    {
        PlayOneShot(matchResultAudioSource, resultLose);
    }

    // === Ã≈“Œƒ€ ƒÀﬂ ¡Œ≈¬€’ «¬” Œ¬ ===

    public void PlayBattleSound()
    {
        PlayOneShot(battleAudioSource, battleSound);
    }

    // === Ã≈“Œƒ€ ƒÀﬂ œŒƒÕﬂ“»ﬂ ﬁÕ»“Œ¬ ===

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
        if (!isInitialized || pickUpUnitAudioSource == null || soundVolume < 0.01f) return;

        // «‡˘ËÚ‡ ÓÚ ÒÔ‡Ï‡
        if (Time.time - lastPickUpTime < pickUpCooldown) return;

        if (pickUpSounds.TryGetValue(unitType.ToLower(), out AudioClip clip) && clip != null)
        {
            pickUpUnitAudioSource.PlayOneShot(clip, soundVolume);
            lastPickUpTime = Time.time;
        }
        else
        {
            Debug.LogWarning($"No pickup sound found for unit type: {unitType}");
        }
    }

    // === Ã≈“Œƒ€ ƒÀﬂ ’ŒƒŒ¬ ===

    public void PlayEndTurnSound()
    {
        PlayOneShot(turnsAudioSource, endTurnSound);
    }

    public void PlaySwitchTurnSound()
    {
        PlayOneShot(turnsAudioSource, switchTurnSound);
    }

    // === Ã≈“Œƒ€ ƒÀﬂ «¬” Œ¬ ’Œƒ‹¡€ ===

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
        if (!isInitialized || walkAudioSource == null || soundVolume < 0.01f) return;

        // «‡˘ËÚ‡ ÓÚ ÒÔ‡Ï‡
        if (Time.time - lastWalkTime < walkCooldown) return;

        if (walkSounds.TryGetValue(unitType.ToLower(), out AudioClip clip) && clip != null)
        {
            walkAudioSource.PlayOneShot(clip, soundVolume);
            lastWalkTime = Time.time;
        }
        else
        {
            Debug.LogWarning($"No walk sound found for unit type: {unitType}");
        }
    }

    // === Ã≈“Œƒ€ ƒÀﬂ —Œ—“ŒﬂÕ»ﬂ ¡»“¬€ ===

    public void PlayBattleLosingSoundStart()
    {
        if (!isInitialized || stateBattleAudioSource == null || battleLosingSound == null) return;

        if (!stateBattleAudioSource.isPlaying)
        {
            stateBattleAudioSource.clip = battleLosingSound;
            stateBattleAudioSource.loop = true;
            stateBattleAudioSource.Play();
        }
    }

    public void PlayBattleWinningSoundStart()
    {
        if (!isInitialized || stateBattleAudioSource == null || battleWinningSound == null) return;

        if (!stateBattleAudioSource.isPlaying)
        {
            stateBattleAudioSource.clip = battleWinningSound;
            stateBattleAudioSource.loop = true;
            stateBattleAudioSource.Play();
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

    // === ¬—œŒÃŒ√¿“≈À‹Õ€≈ Ã≈“Œƒ€ ===

    private void PlayOneShot(AudioSource audioSource, AudioClip clip)
    {
        if (!isInitialized || audioSource == null || clip == null || soundVolume < 0.01f) return;

        audioSource.PlayOneShot(clip, soundVolume);
    }

    // === Ã≈“Œƒ€ ƒÀﬂ ”œ–¿¬À≈Õ»ﬂ √–ŒÃ Œ—“‹ﬁ ===

    public void UpdateSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        ApplyAudioVolumes();
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
    }

    public void UpdateMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyAudioVolumes();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        // ≈ÒÎË ÏÛÁ˚Í‡ ÓÒÚ‡ÌÓ‚ËÎ‡Ò¸, ÌÓ „ÓÏÍÓÒÚ¸ > 0 - ÔÂÂÁ‡ÔÛÒÍ‡ÂÏ
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

    public float GetSoundVolume() => soundVolume;
    public float GetMusicVolume() => musicVolume;

    // === Ã≈“Œƒ€ ƒÀﬂ Œ—“¿ÕŒ¬ » ¬—≈’ «¬” Œ¬ ===

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