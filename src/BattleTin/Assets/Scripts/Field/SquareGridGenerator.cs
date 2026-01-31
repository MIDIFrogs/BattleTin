using System.Collections.Generic;
using UnityEngine;

namespace MIDIFrogs.BattleTin.Field
{
    [ExecuteInEditMode]
    public class SquareGridGenerator : MonoBehaviour
    {
        [Header("Prefabs & Parent")]
        public Hex cellPrefab;
        public Transform parent;

        [Header("Grid Settings")]
        public Vector2Int size = new Vector2Int(8, 8); // W x H
        public Vector3 center;
        public Vector2 spacing = Vector2.one;

        [HideInInspector]
        public List<Hex> generatedCells = new();
    }
}
