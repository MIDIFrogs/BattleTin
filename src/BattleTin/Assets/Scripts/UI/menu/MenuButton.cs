using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button Settings")]
    [SerializeField] private Button button;
    [SerializeField] private ButtonType buttonType;
    [SerializeField] private bool changeIconOnHover = true;

    [Header("Icon Reference")]
    [SerializeField] private IconDisplayManager iconManager;

    [Header("Visual Feedback")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f, 1f);

    [Header("Hover Settings")]
    [SerializeField] private float returnToDefaultDelay = 1f;

    [Header("Line")]
    [SerializeField] private GameObject line;

    private enum ButtonType
    {
        Play,
        Settings,
        Tutorial,
        Exit,
        CancelSearch // Добавлен новый тип
    }

    private bool isPointerOver = false;
    private Coroutine returnToDefaultCoroutine;

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (iconManager == null)
            iconManager = FindObjectOfType<IconDisplayManager>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        button.onClick.AddListener(OnClick);

        if (buttonImage != null)
            buttonImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        // Воспроизводим звук наведения
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHoverSound();

        // Показываем линию если она есть
        if (line != null)
            line.SetActive(true);

        // Отменяем возврат к дефолтной иконке
        if (returnToDefaultCoroutine != null)
        {
            StopCoroutine(returnToDefaultCoroutine);
            returnToDefaultCoroutine = null;
        }

        // Визуальный feedback
        if (buttonImage != null)
            buttonImage.color = hoverColor;

        // Меняем иконку при наведении
        if (changeIconOnHover &&
            MenuManager.Instance.CurrentState != MenuState.SearchingMatch &&
            iconManager != null)
        {
            switch (buttonType)
            {
                case ButtonType.Play:
                    iconManager.ShowPlayIcon();
                    break;
                case ButtonType.Settings:
                    iconManager.ShowSettingsIcon();
                    break;
                case ButtonType.Tutorial:
                    iconManager.ShowTutorialIcon();
                    break;
                case ButtonType.Exit:
                    iconManager.ShowExitIcon();
                    break;
                case ButtonType.CancelSearch:
                    // Для кнопки отмены поиска показываем специальную иконку
                    iconManager.ShowExitIcon(); // Или создайте отдельную иконку
                    break;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPointerOver) return;
        isPointerOver = false;

        // Скрываем линию
        if (line != null)
            line.SetActive(false);

        // Восстанавливаем цвет
        if (buttonImage != null)
            buttonImage.color = normalColor;

        // Запускаем возврат к дефолтной иконке через задержку
        // Только если НЕ в режиме поиска и НЕ кнопка отмены поиска
        if (buttonType != ButtonType.CancelSearch &&
            iconManager != null &&
            MenuManager.Instance.CurrentState != MenuState.SearchingMatch)
        {
            if (returnToDefaultCoroutine != null)
                StopCoroutine(returnToDefaultCoroutine);

            returnToDefaultCoroutine = StartCoroutine(ReturnToDefaultAfterDelay());
        }
    }

    private IEnumerator ReturnToDefaultAfterDelay()
    {
        // Ждем указанное время
        yield return new WaitForSeconds(returnToDefaultDelay);

        // Если все еще не наведено ни на одну кнопку
        if (!isPointerOver && MenuManager.Instance.CurrentState != MenuState.SearchingMatch)
        {
            // Показываем иконку по текущему состоянию
            switch (MenuManager.Instance.CurrentState)
            {
                case MenuState.MainMenu:
                    iconManager.ShowDefaultIcon();
                    break;
                case MenuState.Settings:
                    iconManager.ShowSettingsIcon();
                    break;
                case MenuState.Tutorial:
                    iconManager.ShowTutorialIcon();
                    break;
                case MenuState.SearchingMatch:
                    // В режиме поиска не меняем иконку
                    break;
            }
        }

        returnToDefaultCoroutine = null;
    }

    private void OnClick()
    {
        // Воспроизводим звук клика
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClickSound();

        switch (buttonType)
        {
            case ButtonType.Play:
                MenuManager.Instance.StartGame();
                break;
            case ButtonType.Settings:
                MenuManager.Instance.OpenSettings();
                break;
            case ButtonType.Tutorial:
                MenuManager.Instance.OpenTutorial();
                break;
            case ButtonType.Exit:
                MenuManager.Instance.QuitGame();
                break;
            case ButtonType.CancelSearch:
                MenuManager.Instance.CancelMatchmaking();
                break;
        }
    }

    private void OnDisable()
    {
        isPointerOver = false;
        if (buttonImage != null)
            buttonImage.color = normalColor;

        if (line != null)
            line.SetActive(false);

        if (returnToDefaultCoroutine != null)
        {
            StopCoroutine(returnToDefaultCoroutine);
            returnToDefaultCoroutine = null;
        }
    }
}