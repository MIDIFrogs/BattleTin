using System;
using System.Collections.Generic;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.Editor.Metadata
{
    [Serializable]
    public class LineNodeData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeReference] public DialogAuthor Author { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<DialogResponseData> Responses { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public Sprite FrameSplash { get; set; }
        [field: SerializeField] public AudioClip Voice { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}