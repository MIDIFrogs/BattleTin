using System.Collections.Generic;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.Quests
{
    public static class QuestManager
    {
        private static Dictionary<string, QuestTrigger> triggers;

        public static void Initialize()
        {
            triggers = new();
            foreach (var config in Resources.LoadAll<TriggerConfig>("Triggers"))
            {
                triggers[config.Quest.Tag] = config.Quest;
                config.Quest.IsCompleted = config.InitialValue;
            }
        }

        public static QuestTrigger GetTrigger(string name) => triggers[name];

        public static void ResetTrigger(string triggerName)
        {
            Debug.Log($"Trigger {triggerName} is unset.");
            triggers[triggerName].IsCompleted = false;
        }

        public static void ResetTrigger(QuestTrigger trigger)
        {
            Debug.Log($"Trigger {trigger.Tag} is unset.");
            trigger.IsCompleted = false;
        }

        public static void SetTrigger(string triggerName)
        {
            Debug.Log($"Trigger {triggerName} is set.");
            triggers[triggerName].IsCompleted = true;
        }

        public static void SetTrigger(QuestTrigger trigger)
        {
            Debug.Log($"Trigger {trigger.Tag} is set.");
            trigger.IsCompleted = true;
        }

        public static bool IsTrigger(QuestTrigger trigger)
        {
            return trigger.IsCompleted;
        }
        
        public static bool IsTrigger(string triggerName)
        {
            return triggers[triggerName].IsCompleted;
        }
    }
}
