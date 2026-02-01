using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.Quests.Dialogs
{
    [Serializable]
    public class DialogLine
    {
        [field: SerializeField] public DialogAuthor Author { get; [Obsolete("For editor only")] set; }
        [field: SerializeField, TextArea] public string Message { get; [Obsolete("For editor only")] set; }
        [field: SerializeField] public AudioClip Voice { get; [Obsolete("For editor only")] set; }
        [field: SerializeField] public Sprite FrameSplash { get; [Obsolete("For editor only")] set; }
        [field: SerializeField] public FontStyles FontStyle { get; [Obsolete("For editor only")] set; }
        [field: SerializeField] public string GroupName { get; [Obsolete("For editor only")] set; }
        [field: SerializeReference] public List<Response> Responses { get; [Obsolete("For editor only")] set; } = new();
    }
}
