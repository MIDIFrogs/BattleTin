using System;
using System.Collections.Generic;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.Quests.Dialogs
{
    [Serializable]
    public class Response
    {
        [field: SerializeField] public  List<TriggerConfig> Requirements { get; [Obsolete("For editor only")] set; }
        [field: SerializeField] public TriggerConfig Trigger { get; [Obsolete("For editor only")] set; }
        [field: SerializeField] public string Text { get; [Obsolete("For editor only")] set; }
        [field: SerializeReference] public DialogLine Continuation { get; [Obsolete("For editor only")] set; }
    }
}
