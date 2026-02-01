using UnityEditor;
using UnityEngine;
using MIDIFrogs.BattleTin.Field;
using System.Collections.Generic;

[CustomEditor(typeof(SquareGridGenerator))]
public class SquareGridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        var gen = (SquareGridGenerator)target;

        if (GUILayout.Button("Generate Grid"))
            Generate(gen);

        if (GUILayout.Button("Clear Grid"))
            Clear(gen);
    }

    private void Generate(SquareGridGenerator gen)
    {
        if (gen.cellPrefab == null || gen.parent == null)
        {
            Debug.LogWarning("Cell prefab or parent is not assigned.");
            return;
        }

        Clear(gen);

        int w = gen.size.x;
        int h = gen.size.y;

        var map = new Dictionary<Vector2Int, Hex>();

        Vector3 origin = gen.center -
                         new Vector3(
                             (w - 1) * gen.spacing.x * 0.5f,
                             0,
                             (h - 1) * gen.spacing.y * 0.5f
                         );

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Vector3 pos = origin + new Vector3(
                    x * gen.spacing.x,
                    0,
                    y * gen.spacing.y
                );

                var cell = (Hex)PrefabUtility.InstantiatePrefab(gen.cellPrefab, gen.parent);
                cell.transform.position = pos;

                cell.gridCoordinates = new Vector2Int(x, y);
                cell.Neighbors = new List<Hex>();
                cell.DiagonalNeighbors = new List<Hex>();

                map[cell.gridCoordinates] = cell;
                gen.generatedCells.Add(cell);
            }
        }

        // === связи ===
        foreach (var kv in map)
        {
            var c = kv.Key;
            var cell = kv.Value;

            // прямые соседи (4)
            TryAdd(map, cell.Neighbors, c + Vector2Int.up);
            TryAdd(map, cell.Neighbors, c + Vector2Int.down);
            TryAdd(map, cell.Neighbors, c + Vector2Int.left);
            TryAdd(map, cell.Neighbors, c + Vector2Int.right);

            // диагонали (4)
            TryAdd(map, cell.DiagonalNeighbors, c + new Vector2Int(-1, -1));
            TryAdd(map, cell.DiagonalNeighbors, c + new Vector2Int(-1, +1));
            TryAdd(map, cell.DiagonalNeighbors, c + new Vector2Int(+1, -1));
            TryAdd(map, cell.DiagonalNeighbors, c + new Vector2Int(+1, +1));
        }

        EditorUtility.SetDirty(gen);
    }

    private void Clear(SquareGridGenerator gen)
    {
        if (gen.parent == null)
            return;

        for (int i = gen.parent.childCount - 1; i >= 0; i--)
            DestroyImmediate(gen.parent.GetChild(i).gameObject);

        gen.generatedCells.Clear();
    }

    private void TryAdd(
        Dictionary<Vector2Int, Hex> map,
        List<Hex> list,
        Vector2Int coord
    )
    {
        if (map.TryGetValue(coord, out var cell))
            list.Add(cell);
    }
}
