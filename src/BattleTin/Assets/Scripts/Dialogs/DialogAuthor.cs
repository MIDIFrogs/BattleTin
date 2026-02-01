using UnityEngine;
using System;

namespace MIDIFrogs.FutureInThePast.Quests.Dialogs
{
    [CreateAssetMenu(fileName = "New author", menuName = "Dialogs/Dialog Author")]
    [Serializable]
    public class DialogAuthor : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string authorName;
        [SerializeField] private Color signColor;
        [SerializeField] private Sprite avatar;

        public string Id
        {
            get => id;
            [Obsolete("Only for editor purposes")]
            set => id = value;
        }

        public string Name
        {
            get => authorName;
            [Obsolete("Only for editor purposes")]
            set => authorName = value;
        }

        public Color SignColor
        {
            get => signColor;
            [Obsolete("Only for editor purposes")]
            set => signColor = value;
        }

        public Sprite Avatar
        {
            get => avatar;
            [Obsolete("Only for editor purposes")]
            set => avatar = value;
        }
    }
}