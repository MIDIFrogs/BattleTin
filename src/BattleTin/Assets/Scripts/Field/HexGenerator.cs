using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    using System.Collections.Generic;
    using UnityEngine;

    namespace MIDIFrogs.BattleTin.Field
    {
        [ExecuteInEditMode]
        public class HexGridGenerator : MonoBehaviour
        {
            [Header("Prefabs & Parent")]
            public Hex hexPrefab;
            public Transform parent;

            [Header("Grid Settings")]
            public Vector2Int size = new Vector2Int(5, 5); // W x H
            public Vector3 center;
            public Vector2 spacing = new Vector2(1f, 0.86f);
            // spacing.x Ч рассто€ние по X
            // spacing.y Ч рассто€ние по Z (дл€ гекса)

            [HideInInspector] public List<Hex> generatedHexes = new();
        }
    }
}
