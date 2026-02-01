using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using TMPro;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.UI.Dialogs
{
    public class DialogHistoryEntry : MonoBehaviour
    {
        [Header("Dialog properties")]
        [SerializeField] private TMP_Text AuthorTag;
        [SerializeField] private TMP_Text ReplicText;

        public void PlaceLine(DialogLine line)
        {
            AuthorTag.text = line.Author.Name;
            AuthorTag.color = line.Author.SignColor;

            ReplicText.text = line.Message;
            ReplicText.fontStyle = line.FontStyle;
        }
    }
}
