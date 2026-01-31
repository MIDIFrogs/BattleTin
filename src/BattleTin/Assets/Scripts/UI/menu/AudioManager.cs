using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioSource uiSoundSource; 

    [Header("Sound Clips")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Audio Settings")]
    [SerializeField] private float defaultSoundVolume = 1f;
    [SerializeField] private float defaultMusicVolume = 0.8f;
    [SerializeField] private float uiSoundCooldown = 0.1f; 

    private float soundVolume = 1f;
    private float musicVolume = 0.8f;
    private float lastHoverTime = 0f;
    private float lastClickTime = 0f;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        SetupAudioListeners();
        PlayMusic();
    }

    private void LoadAudioSettings()
    {
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", defaultSoundVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);

        ApplyAudioVolumes();
    }

    private void ApplyAudioVolumes()
    {
        if (soundSource != null)
            soundSource.volume = soundVolume;

        if (uiSoundSource != null)
            uiSoundSource.volume = soundVolume;

        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    private void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicVolume > 0.01f)
        {
            musicSource.Play();
        }
    }

    private void SetupAudioListeners()
    {
        StartCoroutine(SetupListenersDelayed());
    }

    private System.Collections.IEnumerator SetupListenersDelayed()
    {
        yield return new WaitForSeconds(0.1f);

        var buttons = FindObjectsOfType<UnityEngine.UI.Button>(true);
        foreach (var button in buttons)
        {
            AddSoundToButton(button);
        }
    }

    private void AddSoundToButton(UnityEngine.UI.Button button)
    {
        if (button.gameObject.tag == "NoSound") return;
        var eventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();

        // Проверяем, не добавлены ли уже триггеры
        bool hasHover = false;
        bool hasClick = false;

        if (eventTrigger.triggers != null)
        {
            foreach (var entry in eventTrigger.triggers)
            {
                if (entry.eventID == EventTriggerType.PointerEnter) hasHover = true;
                if (entry.eventID == EventTriggerType.PointerClick) hasClick = true;
            }
        }
        else
        {
            eventTrigger.triggers = new List<EventTrigger.Entry>();
        }

        // Добавляем только если их нет
        if (!hasHover)
        {
            var pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => PlayHoverSound());
            eventTrigger.triggers.Add(pointerEnter);
        }

        if (!hasClick)
        {
            var pointerClick = new EventTrigger.Entry();
            pointerClick.eventID = EventTriggerType.PointerClick;
            pointerClick.callback.AddListener((data) => PlayClickSound());
            eventTrigger.triggers.Add(pointerClick);
        }
    }

    public void PlayHoverSound()
    {
        if (!isInitialized) return;

        // Защита от спама - не играем звук чаще чем раз в cooldown
        if (Time.time - lastHoverTime < uiSoundCooldown) return;

        lastHoverTime = Time.time;
        PlayUISound(hoverSound);
    }

    public void PlayClickSound()
    {
        if (!isInitialized) return;

        // Защита от спама
        if (Time.time - lastClickTime < uiSoundCooldown) return;

        lastClickTime = Time.time;
        PlayUISound(clickSound);
    }

    private void PlayUISound(AudioClip clip)
    {
        if (clip == null || soundVolume < 0.01f) return;

        // Используем отдельный источник для UI звуков
        if (uiSoundSource != null)
        {
            uiSoundSource.PlayOneShot(clip, soundVolume);
        }
        else if (soundSource != null)
        {
            soundSource.PlayOneShot(clip, soundVolume);
        }
    }

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

        // Если музыка остановилась, но громкость > 0 - перезапускаем
        if (musicSource != null && !musicSource.isPlaying && musicVolume > 0.01f)
        {
            musicSource.Play();
        }
    }

    public float GetSoundVolume() => soundVolume;
    public float GetMusicVolume() => musicVolume;

    public void AddSoundToNewButton(UnityEngine.UI.Button button)
    {
        if (button != null)
        {
            AddSoundToButton(button);
        }
    }
}