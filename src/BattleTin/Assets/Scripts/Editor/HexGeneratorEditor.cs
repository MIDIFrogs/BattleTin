using System.Collections.Generic;
using MIDIFrogs.BattleTin.Field;
using MIDIFrogs.BattleTin.Field.MIDIFrogs.BattleTin.Field;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGridGenerator))]
public class HexGridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var generator = (HexGridGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Grid"))
        {
            Generate(generator);
        }

        if (GUILayout.Button("Clear Grid"))
        {
            Clear(generator);
        }
    }

    private void Generate(HexGridGenerator gen)
    {
        if (gen.hexPrefab == null || gen.parent == null)
        {
            Debug.LogWarning("Hex prefab or parent is not assigned.");
            return;
        }

        Clear(gen);

        var map = new Dictionary<Vector2Int, Hex>();

        int w = gen.size.x;
        int h = gen.size.y;

        float totalWidth = (w - 1) * gen.spacing.x;
        float totalHeight = (h - 1) * gen.spacing.y;

        Vector3 origin = gen.center - new Vector3(totalWidth / 2f, 0, totalHeight / 2f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float offsetX = (y % 2 == 1) ? gen.spacing.x * 0.5f : 0f;

                Vector3 pos = origin + new Vector3(
                    x * gen.spacing.x + offsetX,
                    0,
                    y * gen.spacing.y
                );

                var hex = (Hex)PrefabUtility.InstantiatePrefab(gen.hexPrefab, gen.parent);
                hex.transform.position = pos;
                hex.gridCoordinates = new Vector2Int(x, y);
                hex.Neighbors = new List<Hex>();
                hex.DiagonalNeighbors = new List<Hex>();

                map[hex.gridCoordinates] = hex;
                gen.generatedHexes.Add(hex);
            }
        }

        // === связи ===
        foreach (var kv in map)
        {
            var c = kv.Key;
            var hex = kv.Value;

            foreach (var dir in GetDirectOffsets(c.y))
            {
                if (map.TryGetValue(c + dir, out var n))
                    hex.Neighbors.Add(n);
            }

            foreach (var dir in GetDiagonalOffsets())
            {
                if (map.TryGetValue(c + dir, out var d))
                    hex.DiagonalNeighbors.Add(d);
            }
        }

        EditorUtility.SetDirty(gen);
    }

    private void Clear(HexGridGenerator gen)
    {
        if (gen.parent == null) return;

        for (int i = gen.parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(gen.parent.GetChild(i).gameObject);
        }

        gen.generatedHexes.Clear();
    }

    private static Vector2Int[] GetDirectOffsets(int row)
    {
        // odd-r horizontal layout
        if (row % 2 == 0)
        {
            return new[]
            {
                new Vector2Int(-1, 0),
                new Vector2Int(+1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, -1),
                new Vector2Int(0, +1),
                new Vector2Int(-1, +1),
            };
        }
        else
        {
            return new[]
            {
                new Vector2Int(-1, 0),
                new Vector2Int(+1, 0),
                new Vector2Int(+1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(+1, +1),
                new Vector2Int(0, +1),
            };
        }
    }

    private static Vector2Int[] GetDiagonalOffsets()
    {
        return new[]
        {
            new Vector2Int(-1, -2),
            new Vector2Int(+1, -2),
            new Vector2Int(-1, +2),
            new Vector2Int(+1, +2),
        };
    }
}
