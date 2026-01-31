using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InputFieldHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_InputField inputField;

    public void Setup(TMP_InputField targetInputField)
    {
        inputField = targetInputField;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inputField != null && inputField.interactable)
        {

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHoverSound();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inputField != null && inputField.interactable)
        {

        }
    }
}