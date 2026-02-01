using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common;
using Cysharp.Threading.Tasks;
using MIDIFrogs.FutureInThePast.Navigation;
using MIDIFrogs.FutureInThePast.Quests;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.UI.Dialogs
{
    public class DialogPlayer : MonoBehaviour
    {
        [SerializeField] private PauseManager pauseMenu;

        [Header("Dialog properties")]
        [SerializeField] private float textSpeed;
        [SerializeField] private bool autoplay;
        [SerializeField] private DialogFrame dialogFrame;

        [Header("Debug options")]
        [SerializeField] private Dialog startupDialog;

        public float TextSpeed => textSpeed;

        public bool Autoplay => autoplay;

        private async void Start()
        {
            autoplay = PlayerPrefs.GetInt("Dialogs.Autoplay", 0) == 1;
            QuestManager.Initialize();
            if (startupDialog != null)
            {
                await StartDialogAsync(startupDialog);
            }
        }

        /// <summary>
        /// Starts a dialog without waiting for the completion
        /// </summary>
        /// <param name="dialog">Dialog to read.</param>
        public async void StartDialog(Dialog dialog)
        {
            await StartDialogAsync(dialog);
        }

        /// <summary>
        /// Starts a dialog and waits for the completion.
        /// </summary>
        /// <param name="dialog">Dialog to read.</param>
        public async UniTask StartDialogAsync(Dialog dialog)
        {
            pauseMenu.IsPaused = true;

            List<DialogLine> history = new();

            var currentLine = dialog.StartLine;
            dialogFrame.gameObject.SetActive(true);

            try
            {
                while (currentLine != null)
                {
                    history.Add(currentLine);
                    var response = await dialogFrame.ShowText(currentLine, history, TextSpeed, Autoplay);
                    if (currentLine.Responses.Count > 0 && response != null)
                    {
                        if (response.Trigger != null && response.Trigger.Quest != null)
                            response.Trigger.Quest.IsCompleted = true;
                        currentLine = response.Continuation;
                    }
                    else
                    {
                        currentLine = null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
                Debug.LogErrorFormat("An error encountered while running a dialog. Exception details: {0}", ex.Message);
            }
            finally
            {
                dialogFrame.gameObject.SetActive(false);
                pauseMenu.IsPaused = false;
            }
        }

        public void SwitchAutoplay(bool autoplay)
        {
            this.autoplay = autoplay;
            PlayerPrefs.SetInt("Dialogs.Autoplay", autoplay ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
