using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace MIDIFrogs.FutureInThePast.UI.Dialogs
{
    public class DialogFrame : MonoBehaviour
    {
        [Header("Dialog properties")]
        [SerializeField] private TMP_Text lineText;
        [SerializeField] private TMP_Text authorName;
        [SerializeField] private Image authorAvatar;
        [SerializeField] private AudioSource voiceOver;
        [SerializeField] private Image splash;
        [SerializeField] private Image oldSplash;

        [Header("History properties")]
        [SerializeField] private DialogHistoryEntry historyEntryPrefab;
        [SerializeField] private GameObject historyPanel;
        [SerializeField] private RectTransform historyViewport;
        
        [Header("Response properties")]
        [SerializeField] private ResponseButton responsePrefab;
        [SerializeField] private GameObject responsePanel;

        private UniTaskCompletionSource<bool> waitButton;

        private ResponseButton[] availableResponses;
        private int selectedResponse;

        public int SelectedResponse
        {
            get => selectedResponse;
            set
            {
                selectedResponse = value;
                for (int i = 0; i < availableResponses.Length; i++)
                {
                    availableResponses[i].IsSelected = i == selectedResponse;
                }
            }
        }
        private void Update()
        {
            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                do
                {
                    SelectedResponse = (SelectedResponse + 1) % availableResponses.Length;
                }
                while (!availableResponses[SelectedResponse].PassesRequirements);
            }
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                do
                {
                    SelectedResponse = (SelectedResponse - 1 + availableResponses.Length) % availableResponses.Length;
                }
                while (!availableResponses[SelectedResponse].PassesRequirements);
            }
            else
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame && availableResponses.Length > 0 && availableResponses[0].PassesRequirements) SelectedResponse = 0;
                if (Keyboard.current.digit2Key.wasPressedThisFrame && availableResponses.Length > 1 && availableResponses[1].PassesRequirements) SelectedResponse = 1;
                if (Keyboard.current.digit3Key.wasPressedThisFrame && availableResponses.Length > 2 && availableResponses[2].PassesRequirements) SelectedResponse = 2;
                if (Keyboard.current.digit4Key.wasPressedThisFrame && availableResponses.Length > 3 && availableResponses[3].PassesRequirements) SelectedResponse = 3;
                if (Keyboard.current.digit5Key.wasPressedThisFrame && availableResponses.Length > 4 && availableResponses[4].PassesRequirements) SelectedResponse = 4;
                if (Keyboard.current.digit6Key.wasPressedThisFrame && availableResponses.Length > 5 && availableResponses[5].PassesRequirements) SelectedResponse = 5;
                if (Keyboard.current.digit7Key.wasPressedThisFrame && availableResponses.Length > 6 && availableResponses[6].PassesRequirements) SelectedResponse = 6;
                if (Keyboard.current.digit8Key.wasPressedThisFrame && availableResponses.Length > 7 && availableResponses[7].PassesRequirements) SelectedResponse = 7;
                if (Keyboard.current.digit9Key.wasPressedThisFrame && availableResponses.Length > 8 && availableResponses[8].PassesRequirements) SelectedResponse = 8;
                if (Keyboard.current.digit0Key.wasPressedThisFrame && availableResponses.Length > 9 && availableResponses[9].PassesRequirements) SelectedResponse = 9;
            }
        }

        /// <summary>
        /// Animates the text writing it asynchronously char-by-char each <see langword="40"/> / <paramref name="textSpeed"/> milliseconds.
        /// </summary>
        /// <param name="text">Text to write.</param>
        /// <param name="textSpeed">Text speed to speed up or slow down the reading.</param>
        /// <param name="token">Token to cancel an animation.</param>
        public async UniTask AnimationLine(string text, float textSpeed, CancellationToken token)
        {
            Debug.Log($"AnimationLine started, working with {textSpeed}.");
            for (int i = 0; i <= text.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                lineText.text = text[..i];
                await UniTask.Delay(TimeSpan.FromMilliseconds(40) / textSpeed, true, cancellationToken: token);
            }
            Debug.Log("AnimationLine completed");

            // Assuming each 100 characters in text should be read in 5 seconds.
            float textReadCoefficient = text.Length / 100f;
            // For autoplay to read after text
            await UniTask.Delay(TimeSpan.FromSeconds(5) * textReadCoefficient, true, cancellationToken: token);
        }

        public async UniTask AnimateImage(Image image, float fromAlpha, float toAlpha, float speed, CancellationToken token)
        {
            for (int i = 0; i < 100; i++)
            {
                token.ThrowIfCancellationRequested();
                float alpha = Mathf.Lerp(fromAlpha, toAlpha, i / 100f);
                image.color = new(1, 1, 1, alpha);
                await UniTask.Delay(TimeSpan.FromMilliseconds(3) / speed, true, cancellationToken: token);
            }
        }

        /// <summary>
        /// Shows the line in the dialog frame.
        /// </summary>
        /// <param name="line">Line to show.</param>
        /// <param name="textSpeed">Reading speed coefficient for the text.</param>
        /// <param name="autoplay">Set to <see langword="true"/> if the line should be autoplayed.</param>
        public async UniTask<Response> ShowText(DialogLine line, List<DialogLine> history, float textSpeed, bool autoplay)
        {
            // Prepare the token to cancel the animation.
            CancellationTokenSource cancelPreTask = new();

            voiceOver.clip = null;
            // Fill up the frame data
            if (line.Voice != null)
            {
                voiceOver.clip = line.Voice;
                voiceOver.Play();
            }
            lineText.fontStyle = line.FontStyle;
            var animation = AnimationLine(line.Message, textSpeed, cancelPreTask.Token);
            authorName.text = line.Author.Name;
            authorAvatar.sprite = line.Author.Avatar;
            authorName.color = line.Author.SignColor;
            UniTask fadeOut = UniTask.CompletedTask, fadeIn = UniTask.CompletedTask;
            // Animate splash screens
            oldSplash.sprite = splash.sprite;
            splash.sprite = line.FrameSplash;

            if (splash.sprite != null)
            {
                splash.color = new Color(1, 1, 1, 0);
                fadeIn = AnimateImage(splash, 0, 1, textSpeed, cancelPreTask.Token);
                if (oldSplash.sprite != null)
                {
                    oldSplash.color = Color.white;
                }
            }
            else
            {
                splash.color = default;
                if (oldSplash.sprite != null)
                {
                    oldSplash.color = Color.white;
                    fadeOut = AnimateImage(oldSplash, 1, 0, textSpeed, cancelPreTask.Token);
                }
                else
                {
                    oldSplash.color = default;
                }
            }

            foreach (Transform child in historyViewport)
            {
                Destroy(child.gameObject);
            }
            foreach (var histLine in history)
            {
                var lineEntry = Instantiate(historyEntryPrefab, historyViewport);
                lineEntry.PlaceLine(histLine);
            }
            foreach (Transform child in responsePanel.transform)
            {
                Destroy(child.gameObject);
            }
            List<UniTask> buttonTasks = new();
            availableResponses = new ResponseButton[line.Responses.Count];
            int i = 0;
            foreach (var response in line.Responses)
            {
                var button = Instantiate(responsePrefab, responsePanel.transform);
                availableResponses[i++] = button;
                buttonTasks.Add(button.WaitForClick(response));
            }
            SelectedResponse = Array.FindIndex(availableResponses, x => x.PassesRequirements);
            int selectedButton;
            // Check if we can autoplay
            if (buttonTasks.Count == 1)
            {
                // Wait for the button click
                if (autoplay)
                {
                    await UniTask.WhenAny(UniTask.WhenAll(animation, fadeIn, fadeOut), buttonTasks[0]);
                }
                else
                {
                    await buttonTasks[0];
                }
                selectedButton = 0;
            }
            else
            {
                selectedButton = await UniTask.WhenAny(buttonTasks);
            }
            Debug.Log($"Selected button index: {selectedButton}");

            // Cancel incomplete animations
            cancelPreTask.Cancel();
            if (line.Voice != null)
            {
                voiceOver.Stop();
            }

            return line.Responses.ElementAtOrDefault(selectedButton);
        }
    }
}