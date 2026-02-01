using MIDIFrogs.BattleTin.Field;
using UnityEngine;
using UnityEngine.UI;

public class TurnManagerUI : MonoBehaviour
{
    [Header("Спрайты для таймера")]
    [SerializeField] private Image fillSprite;
    [SerializeField] private Image backgroundSprite; 

    [Header("Настройки")]
    [SerializeField] private bool autoSubscribe = true; 

    private TurnController turnController;
    private float currentDuration;
    private float timer;
    private bool isTimerActive;

    private void Start()
    {
        if (autoSubscribe)
        {
            FindAndSubscribeToTurnController();
        }

        // Скрываем таймер до старта
        SetTimerVisible(false);
    }

    private void Update()
    {
        if (!isTimerActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = 0;
            isTimerActive = false;
            SetTimerVisible(false);
        }

        UpdateTimerVisual();
    }

    /// <summary>
    /// Поиск и подписка на TurnController
    /// </summary>
    private void FindAndSubscribeToTurnController()
    {
        if (turnController != null)
        {
            turnController.TurnTimerStarted += OnTurnTimerStarted;
        }
        else
        {
            Debug.LogWarning("TurnTimerUI: TurnController не найден на сцене!");
        }
    }

    /// <summary>
    /// Обработчик события начала таймера
    /// </summary>
    /// <param name="duration">Длительность таймера</param>
    protected virtual void OnTurnTimerStarted(float duration)
    {
        StartTimer(duration);
    }

    /// <summary>
    /// Запуск таймера
    /// </summary>
    /// <param name="duration">Длительность в секундах</param>
    public void StartTimer(float duration)
    {
        if (fillSprite == null)
        {
            Debug.LogError("TurnTimerUI: Fill sprite не назначен!");
            return;
        }

        currentDuration = duration;
        timer = duration;
        isTimerActive = true;

        SetTimerVisible(true);
        UpdateTimerVisual();
    }

    /// <summary>
    /// Обновление визуального отображения таймера
    /// </summary>
    private void UpdateTimerVisual()
    {
        if (fillSprite == null) return;

        // Рассчитываем прогресс от 1 до 0
        float progress = timer / currentDuration;

        // Обновляем заполнение спрайта
        fillSprite.fillAmount = progress;

        // Опционально: меняем цвет в зависимости от оставшегося времени
        UpdateColorBasedOnProgress(progress);
    }

    /// <summary>
    /// Обновление цвета в зависимости от прогресса
    /// </summary>
    private void UpdateColorBasedOnProgress(float progress)
    {
        if (fillSprite == null) return;

        // Пример: от зеленого к красному при истечении времени
        if (progress > 0.5f)
        {
            fillSprite.color = Color.Lerp(Color.yellow, Color.green, (progress - 0.5f) * 2);
        }
        else
        {
            fillSprite.color = Color.Lerp(Color.red, Color.yellow, progress * 2);
        }
    }

    /// <summary>
    /// Управление видимостью элементов таймера
    /// </summary>
    private void SetTimerVisible(bool isVisible)
    {
        if (fillSprite != null)
            fillSprite.gameObject.SetActive(isVisible);

        if (backgroundSprite != null)
            backgroundSprite.gameObject.SetActive(isVisible);
    }

    /// <summary>
    /// Ручная подписка на TurnController
    /// </summary>
    public void SubscribeToTurnController(TurnController controller)
    {
        if (controller != null)
        {
            turnController = controller;
            turnController.TurnTimerStarted += OnTurnTimerStarted;
        }
    }

    /// <summary>
    /// Остановка таймера
    /// </summary>
    public void StopTimer()
    {
        isTimerActive = false;
        SetTimerVisible(false);
    }

    /// <summary>
    /// Проверка, активен ли таймер
    /// </summary>
    public bool IsTimerActive()
    {
        return isTimerActive;
    }

    /// <summary>
    /// Получение оставшегося времени
    /// </summary>
    public float GetRemainingTime()
    {
        return timer;
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        if (turnController != null)
        {
            turnController.TurnTimerStarted -= OnTurnTimerStarted;
        }
    }
}