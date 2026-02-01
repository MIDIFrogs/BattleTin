using System;
using System.Collections.Generic;
using MIDIFrogs.FutureInThePast.Quests;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.Editor.Metadata
{
    [Serializable]
    public class DialogResponseData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
        [field: SerializeField] public TriggerConfig Trigger { get; set; }
        [field: SerializeField] public List<TriggerConfig> Requirements { get; set; }
    }
}