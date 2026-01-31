using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Slider slider;

    public void Setup(Slider targetSlider)
    {
        slider = targetSlider;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slider != null && slider.interactable)
        {

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHoverSound();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slider != null && slider.interactable)
        {
        }
    }
}