using System;
using System.Collections.Generic;
using UnityEngine;

namespace MIDIFrogs.FutureInThePast.Quests
{
    [Serializable]
    public class CompoundTrigger
    {
        [Serializable]
        public enum Operation
        {
            AND,
            OR,
            NOT
        }

        [Serializable]
        public class Node
        {
            public QuestTrigger Trigger;
            public Operation Operation;
            public List<Node> Children = new List<Node>();
        }

        public Node RootNode = new();

        public bool Evaluate()
        {
            return EvaluateNode(RootNode);
        }

        private bool EvaluateNode(Node node)
        {
            if (node == null) return false;

            bool result = node.Trigger?.IsCompleted ?? false;

            foreach (var child in node.Children)
            {
                bool childResult = EvaluateNode(child);
                switch (node.Operation)
                {
                    case Operation.AND:
                        result = result && childResult;
                        break;
                    case Operation.OR:
                        result = result || childResult;
                        break;
                    case Operation.NOT:
                        result = !childResult;
                        break;
                }
            }

            return result;
        }
    }
}