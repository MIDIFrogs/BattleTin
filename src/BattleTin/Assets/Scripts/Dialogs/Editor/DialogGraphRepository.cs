using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.FutureInThePast.Editor.Metadata;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS0618 // SO editor warnings
namespace MIDIFrogs.FutureInThePast.Editor.Dialogs
{
    public class DialogGraphRepository
    {
        private const string CacheFolderName = "DialogsCache";
        private const string CachePath = "Assets/Editor Default Resources/Dialogs/" + CacheFolderName;
        private const string DialogPath = "Assets/Resources/Dialogs";

        private readonly Dialog _dialog;
        private readonly string _fileName;
        private readonly Dictionary<string, LineNode> _nodeMap = new();
        private readonly DialogGraphView _view;
        private DialogGraphData _cache;

        public DialogGraphRepository(DialogGraphView view, Dialog dialog)
        {
            _view = view;
            _dialog = dialog;
            _fileName = dialog.name;
            EnsureCacheExists();
        }

        public static string GetCachePath(Dialog dialog)
        {
            // create folders if needed
            if (!AssetDatabase.IsValidFolder("Assets/Editor Default Resources/Dialogs"))
                AssetDatabase.CreateFolder("Assets/Editor Default Resources/", "Dialogs");
            if (!AssetDatabase.IsValidFolder(CachePath))
                AssetDatabase.CreateFolder("Assets/Editor Default Resources/Dialogs", CacheFolderName);

            return $"{CachePath}/{dialog.name}.asset";
        }

        /// <summary>
        /// Populate the GraphView from cache, using CreateGroupAt/CreateNodeAt.
        /// </summary>
        public void Load()
        {
            _view.ClearGraph();
            _nodeMap.Clear();

            Dictionary<string, LinesGroup> groups = new();
            // groups
            foreach (var grpData in _cache.Groups)
            {
                var grp = _view.CreateGroupAt(grpData.Name, grpData.Position);
                grp.ID = grpData.ID;
                groups[grp.ID] = grp;
            }

            // nodes
            foreach (var lineData in _cache.Lines)
            {
                var node = _view.CreateNodeAt(lineData.Position, true);
                node.ID = lineData.ID;
                node.Author = lineData.Author;
                node.Text = lineData.Text;
                node.Responses = lineData.Responses;
                node.Voice = lineData.Voice;
                node.FrameSplash = lineData.FrameSplash;

                // assign group
                if (!string.IsNullOrEmpty(lineData.GroupID) && groups.TryGetValue(lineData.GroupID, out var grp))
                {
                    try
                    {
                        node.Group = grp;
                        grp.AddElement(node);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }

                node.Initialize(_view, lineData.Position);
                _nodeMap[lineData.ID] = node;
            }

            // reconnect
            foreach (var node in _nodeMap.Values)
            {
                for (int i = 0; i < node.Responses.Count; i++)
                {
                    var resp = node.Responses[i];
                    if (string.IsNullOrEmpty(resp.NodeID) || !_nodeMap.TryGetValue(resp.NodeID, out var dest))
                        continue;

                    var outputPort = node.outputContainer[i] as UnityEditor.Experimental.GraphView.Port;
                    var inputPort = dest.inputContainer[0] as UnityEditor.Experimental.GraphView.Port;
                    var edge = outputPort.ConnectTo(inputPort);
                    _view.Add(edge);
                }
            }

            _view.OnGraphChanged();
        }

        /// <summary>
        /// Serialize the GraphView back to cache and Dialog asset.
        /// </summary>
        public void Save()
        {
            Undo.RecordObject(_cache, "Update Dialog Cache");
            _cache.Groups.Clear();
            _cache.Lines.Clear();

            // persist groups
            foreach (var grp in _view.graphElements.OfType<LinesGroup>())
            {
                _cache.Groups.Add(new DialogGroupData
                {
                    ID = grp.ID,
                    Name = grp.title,
                    Position = grp.GetPosition().position
                });
            }

            // persist nodes
            foreach (var node in _view.graphElements.OfType<LineNode>())
            {
                _cache.Lines.Add(new LineNodeData
                {
                    ID = node.ID,
                    Author = node.Author,
                    Text = node.Text,
                    GroupID = node.Group?.ID,
                    Position = node.GetPosition().position,
                    Voice = node.Voice,
                    FrameSplash = node.FrameSplash,
                    Responses = node.Responses
                });
            }

            EditorUtility.SetDirty(_cache);

            // rebuild Dialog asset
            Undo.RecordObject(_dialog, "Save Dialog Asset");
            var lookup = new Dictionary<string, DialogLine>();
            foreach (var ld in _cache.Lines)
            {
                var dl = new DialogLine
                {
                    Author = ld.Author,
                    Message = ld.Text,
                    Voice = ld.Voice,
                    FrameSplash = ld.FrameSplash,
                    FontStyle = TMPro.FontStyles.Normal,
                    GroupName = null,
                    Responses = new List<Response>()
                };
                lookup[ld.ID] = dl;
            }

            // create responses
            foreach (var ld in _cache.Lines)
            {
                var source = lookup[ld.ID];
                foreach (var rd in ld.Responses)
                {
                    source.Responses.Add(new Response
                    {
                        Text = rd.Text,
                        Trigger = rd.Trigger,
                        Requirements = rd.Requirements,
                        Continuation = string.IsNullOrEmpty(rd.NodeID) ? null : lookup[rd.NodeID]
                    });
                }
            }

            // determine StartLine (root)
            var allTargets = new HashSet<string>(_cache.Lines.SelectMany(l => l.Responses.Select(r => r.NodeID)).Where(id => !string.IsNullOrEmpty(id)));
            var rootData = _cache.Lines.FirstOrDefault(l => !allTargets.Contains(l.ID));
            _dialog.StartLine = lookup[rootData?.ID ?? _cache.Lines.First().ID];

            EditorUtility.SetDirty(_dialog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void BuildCacheFromDialog()
        {
            // bootstrap new dialog
            var ids = new Dictionary<DialogLine, string>();
            Traverse(_dialog.StartLine, ids);

            // create LineNodeData entries
            foreach (var kv in ids)
            {
                var line = kv.Key;
                _cache.Lines.Add(new LineNodeData
                {
                    ID = kv.Value,
                    Author = line.Author,
                    Text = line.Message,
                    Position = Vector2.zero,
                    GroupID = null,
                    Voice = line.Voice,
                    FrameSplash = line.FrameSplash,
                    Responses = line.Responses.Select(r => new DialogResponseData
                    {
                        Text = r.Text,
                        NodeID = null,
                        Trigger = r.Trigger,
                        Requirements = r.Requirements
                    }).ToList()
                });
            }

            // fill continuation IDs
            foreach (var nd in _cache.Lines)
            {
                var line = ids.First(k => k.Value == nd.ID).Key;
                for (int i = 0; i < line.Responses.Count; i++)
                    nd.Responses[i].NodeID = ids[line.Responses[i].Continuation];
            }
        }

        private void EnsureCacheExists()
        {
            _cache = AssetDatabase.LoadAssetAtPath<DialogGraphData>($"{CachePath}/{_fileName}.asset");
            if (_cache != null)
                return;

            // create folders if needed
            if (!AssetDatabase.IsValidFolder("Assets/Editor/Dialogs"))
                AssetDatabase.CreateFolder("Assets/Editor", "Dialogs");
            if (!AssetDatabase.IsValidFolder(CachePath))
                AssetDatabase.CreateFolder("Assets/Editor/Dialogs", CacheFolderName);

            // make new cache
            _cache = ScriptableObject.CreateInstance<DialogGraphData>();
            _cache.Initialize(_fileName);
            AssetDatabase.CreateAsset(_cache, $"{CachePath}/{_fileName}.asset");
            AssetDatabase.SaveAssets();

            // bootstrap from dialog
            BuildCacheFromDialog();
            EditorUtility.SetDirty(_cache);
            AssetDatabase.SaveAssets();
        }

        private void Traverse(DialogLine line, Dictionary<DialogLine, string> map)
        {
            if (line == null || map.ContainsKey(line)) return;
            map[line] = Guid.NewGuid().ToString();
            foreach (var resp in line.Responses)
                Traverse(resp.Continuation, map);
        }
    }
}