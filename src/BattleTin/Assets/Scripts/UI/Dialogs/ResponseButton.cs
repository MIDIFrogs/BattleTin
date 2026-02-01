using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MIDIFrogs.FutureInThePast.UI.Dialogs
{
    public class ResponseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string requirementsLabel;

        [Header("Dialog GUI properties")]
        [SerializeField] private TMP_Text responseText;
        [SerializeField] private TMP_Text requirementsText;
        [SerializeField] private GameObject selectionTint;
        [SerializeField] private Button button;
        [SerializeField] private Image indicatorImage;
        [SerializeField] private Color availableColor = Color.green;
        [SerializeField] private Color unavailableColor = Color.red;

        private readonly UniTaskCompletionSource<bool> clickTcs = new();

        private bool passesRequirements;
        private bool isSelected;

        public bool PassesRequirements => passesRequirements;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                selectionTint.SetActive(value);
            }
        }

        private void LateUpdate()
        {
            if (IsSelected && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                Click();
            }
        }

        public async UniTask WaitForClick(Response response)
        {
            responseText.text = response.Text;
            //Debug.Log($"Found requirements: {string.Join(',', response.Requirements.Select(x => x.Quest.Tag + ":" + x.Quest.IsCompleted))}");
            passesRequirements = response.Requirements.All(x => x.Quest.IsCompleted);
            button.interactable = passesRequirements;
            if (!passesRequirements)
            {
                requirementsText.text = requirementsLabel + string.Join(Environment.NewLine, response.Requirements.Where(x => !x.Quest.IsCompleted).Select(x => x.Quest.Description));
                indicatorImage.color = unavailableColor;
            }
            else
            {
                requirementsText.text = string.Empty;
                indicatorImage.color = availableColor;
            }
            await clickTcs.Task;
        }

        public void Click()
        {
            clickTcs.TrySetResult(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!passesRequirements)
            {
                responseText.gameObject.SetActive(true);
                requirementsText.gameObject.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!passesRequirements)
            {
                responseText.gameObject.SetActive(false);
                requirementsText.gameObject.SetActive(true);
            }
        }
    }
}
