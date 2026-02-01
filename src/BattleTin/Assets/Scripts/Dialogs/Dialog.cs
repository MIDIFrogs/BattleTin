using UnityEngine;
using System;

namespace MIDIFrogs.FutureInThePast.Quests.Dialogs
{
    [Serializable]
    [CreateAssetMenu(fileName = "New dialog", menuName = "Dialogs/Dialog")]
    public class Dialog : ScriptableObject
    {
        [field: SerializeReference] public DialogLine StartLine { get; [Obsolete("For editor only")] set; }
    }
}
