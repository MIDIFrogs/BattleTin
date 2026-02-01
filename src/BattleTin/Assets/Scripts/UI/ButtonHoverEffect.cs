using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprites Settings")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;

    private Image buttonImage;
    private Button button;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        button = GetComponent<Button>();

        if (normalSprite == null)
        {
            normalSprite = buttonImage.sprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        if (hoverSprite != null)
        {
            buttonImage.sprite = hoverSprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (normalSprite != null)
        {
            buttonImage.sprite = normalSprite;
        }
    }

    // Опционально: сброс спрайта при отключении кнопки
    private void OnDisable()
    {
        if (normalSprite != null)
        {
            buttonImage.sprite = normalSprite;
        }
    }
}