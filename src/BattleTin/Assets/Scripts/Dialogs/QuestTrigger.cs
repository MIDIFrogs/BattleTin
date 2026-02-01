using System;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.Quests
{
    [Serializable]
    public class QuestTrigger
    {
        private bool isCompleted;
        
        public event EventHandler Completed;

        public event EventHandler Restored;

        [field: SerializeField] public string Tag { get; [Obsolete("For editor only")] set; }
        [field: SerializeField] public string Description { get; [Obsolete("For editor only")] set; }

        public bool IsCompleted 
        {
            get => isCompleted;
            set
            {
                if (isCompleted != value)
                {
                    isCompleted = value;
                    if (isCompleted)
                    {
                        Completed?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        Restored?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
