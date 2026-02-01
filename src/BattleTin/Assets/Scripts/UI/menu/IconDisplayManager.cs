using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class IconDisplayManager : MonoBehaviour
{
    [Header("Icon Display")]
    [SerializeField] public Image iconImage;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite settingsIcon;
    [SerializeField] private Sprite tutorialIcon;
    [SerializeField] private Sprite exitIcon;

    [Header("Search Elements")]
    [SerializeField] public Image spinnerImage;
    [SerializeField] public TextMeshProUGUI searchTimerText;

    [Header("Animation Settings")]
    [SerializeField] private float iconTransitionTime = 0.3f;
    [SerializeField] private float fadeTime = 0.2f;
    [SerializeField] private float spinnerSpeed = 180f;

    private Coroutine currentTransition;
    private Coroutine fadeOutCoroutine;
    private bool isSearching = false;
    private Vector3 originalScale;
    private Color originalIconColor;

    // Текущий тип иконки
    private IconType currentIconType = IconType.Default;

    private enum IconType
    {
        Default,
        Play,
        Settings,
        Tutorial,
        Exit,
        Searching
    }

    private void Start()
    {
        // Сохраняем оригинальный масштаб и цвет
        originalScale = iconImage.rectTransform.localScale;
        originalIconColor = iconImage.color;

        // Скрываем элементы поиска
        if (spinnerImage != null)
            spinnerImage.gameObject.SetActive(false);

        if (searchTimerText != null)
            searchTimerText.gameObject.SetActive(false);

        // Показываем дефолтную иконку
        ShowDefaultIcon();
    }

    private void OnDisable()
    {
        // При отключении останавливаем все корутины
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        // Восстанавливаем масштаб и цвет
        iconImage.rectTransform.localScale = originalScale;
        iconImage.color = originalIconColor;
    }

    private void Update()
    {
        if (isSearching && spinnerImage != null && spinnerImage.gameObject.activeInHierarchy)
        {
            spinnerImage.transform.Rotate(0, 0, -spinnerSpeed * Time.deltaTime);
        }
    }

    public void ShowDefaultIcon()
    {
        if (isSearching) return; // Не меняем иконку во время поиска

        ChangeIcon(IconType.Default, defaultIcon);
    }

    private void ChangeIcon(IconType newType, Sprite newIcon)
    {
        // Если уже такая же иконка - игнорируем
        if (currentIconType == newType && !isSearching)
            return;

        // Если в процессе поиска - не меняем обычные иконки
        if (isSearching && newType != IconType.Default && newType != IconType.Searching)
            return;

        currentIconType = newType;

        // Останавливаем все анимации
        StopAllCoroutinesForIcon();

        // Восстанавливаем полную непрозрачность
        iconImage.color = originalIconColor;

        if (newIcon != null)
        {
            currentTransition = StartCoroutine(ChangeIconWithScaleSmooth(newIcon));
        }
        else
        {
            iconImage.sprite = null;
        }
    }

    private void StopAllCoroutinesForIcon()
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        // Восстанавливаем нормальный масштаб и цвет
        iconImage.rectTransform.localScale = originalScale;
        iconImage.color = originalIconColor;
    }

    private IEnumerator ChangeIconWithScaleSmooth(Sprite newIcon)
    {
        RectTransform rt = iconImage.rectTransform;
        rt.localScale = originalScale;

        // Шаг 1: Исчезновение
        float elapsed = 0f;
        Vector3 targetScale = originalScale * 0.7f;
        Color startColor = iconImage.color;
        Color transparentColor = new Color(startColor.r, startColor.g, startColor.b, 0.3f);

        while (elapsed < iconTransitionTime / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (iconTransitionTime / 2);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            rt.localScale = Vector3.Lerp(originalScale, targetScale, smoothT);
            iconImage.color = Color.Lerp(startColor, transparentColor, smoothT);

            yield return null;
        }

        // Меняем спрайт
        iconImage.sprite = newIcon;

        // Шаг 2: Появление
        elapsed = 0f;
        Vector3 startScale = rt.localScale;
        Color startFadeColor = iconImage.color;

        while (elapsed < iconTransitionTime / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (iconTransitionTime / 2);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Легкий "овершута" и возврат
            float overshoot = 1.1f;
            float scaleT = smoothT;

            if (t < 0.8f)
            {
                rt.localScale = Vector3.Lerp(startScale, originalScale * overshoot, scaleT);
            }
            else
            {
                rt.localScale = Vector3.Lerp(rt.localScale, originalScale, (t - 0.8f) * 5f);
            }

            iconImage.color = Color.Lerp(startFadeColor, originalIconColor, smoothT);

            yield return null;
        }

        // Гарантируем точный конечный размер и цвет
        rt.localScale = originalScale;
        iconImage.color = originalIconColor;

        currentTransition = null;
    }

    // Публичные методы для смены иконок
    public void ShowPlayIcon()
    {
        ChangeIcon(IconType.Play, playIcon);
    }

    public void ShowSettingsIcon()
    {
        ChangeIcon(IconType.Settings, settingsIcon);
    }

    public void ShowTutorialIcon()
    {
        ChangeIcon(IconType.Tutorial, tutorialIcon);
    }

    public void ShowExitIcon()
    {
        ChangeIcon(IconType.Exit, exitIcon);
    }

    public void ShowSearchIcon()
    {
        isSearching = true;
        currentIconType = IconType.Searching;

        // Останавливаем все анимации
        StopAllCoroutinesForIcon();

        // Показываем спинер
        if (spinnerImage != null)
        {
            spinnerImage.gameObject.SetActive(true);
            spinnerImage.transform.rotation = Quaternion.identity;
        }

        if (searchTimerText != null)
        {
            searchTimerText.gameObject.SetActive(true);
            searchTimerText.text = "00:00";
        }

        // Показываем иконку поиска (например, дефолтную)
        if (defaultIcon != null)
        {
            iconImage.sprite = defaultIcon;
            // Делаем иконку полупрозрачной
            fadeOutCoroutine = StartCoroutine(FadeIconToAlpha(0.3f));
        }
    }

    private IEnumerator FadeIconToAlpha(float targetAlpha)
    {
        float elapsed = 0f;
        Color startColor = iconImage.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            iconImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
    }

    public void HideSearchIcon()
    {
        if (!isSearching) return;

        isSearching = false;

        // Скрываем спинер
        if (spinnerImage != null)
        {
            spinnerImage.gameObject.SetActive(false);
            spinnerImage.transform.rotation = Quaternion.identity;
        }

        if (searchTimerText != null)
        {
            searchTimerText.gameObject.SetActive(false);
        }

        // Возвращаем нормальную иконку
        StartCoroutine(FadeIconToAlpha(1f));

        // Через небольшую задержку показываем дефолтную иконку
        StartCoroutine(ShowDefaultAfterDelay(0.1f));
    }

    private IEnumerator ShowDefaultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowDefaultIcon();
    }

    public void UpdateSearchTimer(int secondsElapsed)
    {
        if (searchTimerText != null && searchTimerText.gameObject.activeInHierarchy)
        {
            int minutes = secondsElapsed / 60;
            int seconds = secondsElapsed % 60;
            searchTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}