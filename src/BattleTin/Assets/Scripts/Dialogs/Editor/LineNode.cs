using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.FutureInThePast.Editor.Metadata;
using MIDIFrogs.FutureInThePast.Editor.Utilities;
using MIDIFrogs.FutureInThePast.Quests;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MIDIFrogs.FutureInThePast.Editor.Dialogs
{
    public class LineNode : Node
    {
        private DialogGraphView _graphView;
        public DialogAuthor Author { get; set; }
        public TMPro.FontStyles FontStyle { get; set; }
        public Sprite FrameSplash { get; set; }
        public LinesGroup Group { get; set; }
        public string GroupName => Group?.title ?? string.Empty;
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public List<DialogResponseData> Responses { get; set; } = new() { new() { Text = "Continue" } };
        public string Text { get; set; }
        public AudioClip Voice { get; set; }

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        public void Initialize(DialogGraphView graphView, Vector2 position)
        {
            _graphView = graphView;
            SetPosition(new Rect(position, Vector2.zero));
            style.width = 300;

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            // Title and input port
            var title = (Label)titleContainer[0];
            title.text = "Dialog Line";
            title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            title.style.fontSize = 14;
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            input.AddManipulator(new EdgeConnector<Edge>(new GraphUpdateListener(this)));
            input.portName = "";
            inputContainer.Add(input);

            // Main foldable body
            var bodyFold = new Foldout { text = "Line Details", value = true };
            var body = new VisualElement { style = { flexDirection = FlexDirection.Column } };

            // Author
            var authField = new ObjectField("Author") { objectType = typeof(DialogAuthor), value = Author };
            authField.RegisterValueChangedCallback(evt => Author = (DialogAuthor)evt.newValue);
            body.Add(authField);

            // Dialog Message Foldout (multiline)
            var textFoldout = body.CreateFoldout("Message");

            // Use CreateTextArea for proper multi-line textarea
            var textArea = textFoldout.CreateTextArea(Text, null, evt => Text = evt.newValue);
            textArea.style.minHeight = 60;
            textArea.style.flexGrow = 1;
            textArea.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            // Responses foldout
            var respFold = new Foldout { text = "Responses", value = true };
            var respContainer = new VisualElement { style = { flexDirection = FlexDirection.Column } };
            var addResp = new Button(() =>
            {
                var newResp = new DialogResponseData { Text = "Choice" };
                Responses.Add(newResp);
                respContainer.Add(CreateResponseFoldout(newResp));
            })
            { text = "Add Response" };
            respContainer.Add(addResp);
            foreach (var resp in Responses)
                respContainer.Add(CreateResponseFoldout(resp));
            respFold.Add(respContainer);
            body.Add(respFold);

            // Additional properties fold
            var extraFold = new Foldout { text = "Extra Properties", value = false };
            var extra = new VisualElement { style = { flexDirection = FlexDirection.Column } };
            var voiceField = new ObjectField("Voice") { objectType = typeof(AudioClip), value = Voice };
            voiceField.RegisterValueChangedCallback(evt => Voice = (AudioClip)evt.newValue);
            extra.Add(voiceField);
            var frameField = new ObjectField("FrameSplash") { objectType = typeof(Sprite), value = FrameSplash };
            frameField.RegisterValueChangedCallback(evt => FrameSplash = (Sprite)evt.newValue);
            extra.Add(frameField);
            var fontField = new EnumField("Font Style", FontStyle);
            fontField.RegisterValueChangedCallback(evt => FontStyle = (TMPro.FontStyles)evt.newValue);
            extra.Add(fontField);
            var groupField = new TextField("Group Name") { value = GroupName };
            groupField.SetEnabled(false);
            //groupField.RegisterValueChangedCallback(evt =>
            //{
            //    if (Group == null)
            //    {
            //        try
            //        {
            //            var group = graphView.CreateGroupAt(evt.newValue, position);
            //            group.AddElement(this);
            //        }
            //        catch // Because I don't know why, but adding elements from the graph to group produces an error.
            //        {
            //        }
            //    }
            //    else
            //    {
            //        Group.title = evt.newValue;
            //    }
            //});
            extra.Add(groupField);
            extraFold.Add(extra);
            body.Add(extraFold);

            bodyFold.Add(body);
            extensionContainer.Add(bodyFold);

            RefreshExpandedState();
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = new Color(29 / 255f, 29 / 255f, 30 / 255f);
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        private VisualElement CreateResponseFoldout(DialogResponseData resp)
        {
            var foldStack = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            var fold = new Foldout { text = resp.Text, value = false };
            var container = new VisualElement { style = { flexDirection = FlexDirection.Column } };

            void deletePort()
            {
                if (Responses.Count == 1)
                {
                    return;
                }

                var port = outputContainer.Children().OfType<Port>().FirstOrDefault(p => p.userData == resp);
                if (port != null) _graphView.RemoveElement(port);

                if (port.connected)
                {
                    _graphView.DeleteElements(port.connections);
                }

                // Remove DTO
                Responses.Remove(resp);

                // Remove port
                _graphView.RemoveElement(port);

                // Remove UI fold
                foldStack.RemoveFromHierarchy();
            }

            Button deleteChoiceButton = foldStack.CreateButton("x", deletePort);
            deleteChoiceButton.style.maxHeight = deleteChoiceButton.style.maxWidth = 16;
            deleteChoiceButton.AddToClassList("ds-node__button");

            var ctx = new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Remove Response", _ => deletePort(), DropdownMenuAction.Status.Normal);
            });

            // Context menu for removal
            fold.AddManipulator(ctx);

            // Text
            var txt = container.CreateTextField(resp.Text, null, evt =>
            {
                resp.Text = evt.newValue;
                fold.text = evt.newValue;
                var port = outputContainer.Children().OfType<Port>().FirstOrDefault(p => p.userData == resp);
                if (port != null) port.portName = resp.Text;
            });
            txt.AddClasses("ds-node__text-field", "ds-node__choice-text-field");

            // Trigger
            var trig = new ObjectField("Trigger") { objectType = typeof(TriggerConfig), value = resp.Trigger };
            trig.RegisterValueChangedCallback(evt => resp.Trigger = (TriggerConfig)evt.newValue);
            container.Add(trig);

            // Requirements
            var reqFold = new Foldout { text = "Requirements", value = false };
            var addReq = new Button(() =>
            {
                resp.Requirements ??= new List<TriggerConfig>();
                resp.Requirements.Add(null);
                var istack = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
                Button deleteResponseButton = istack.CreateButton("X", () =>
                {
                    resp.Requirements.RemoveAt(resp.Requirements.Count - 1);
                    reqFold.Remove(istack);
                });
                deleteResponseButton.AddToClassList("ds-node__button");
                var of = new ObjectField { objectType = typeof(TriggerConfig), value = null };
                of.RegisterValueChangedCallback(e => { resp.Requirements[^1] = (TriggerConfig)e.newValue; });
                istack.Add(of);
                reqFold.Add(istack);
            })
            { text = "Add Requirement" };
            reqFold.Add(addReq);
            int i = 0;
            foreach (var req in resp.Requirements ?? new List<TriggerConfig>())
            {
                int index = i++;
                var istack = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
                Button deleteResponseButton = istack.CreateButton("X", () =>
                {
                    resp.Requirements.Remove(req);
                    reqFold.Remove(istack);
                });
                deleteResponseButton.AddToClassList("ds-node__button");
                var of = new ObjectField { objectType = typeof(TriggerConfig), value = req };
                of.RegisterValueChangedCallback(e => { resp.Requirements[index] = (TriggerConfig)e.newValue; });
                istack.Add(of);
                reqFold.Add(istack);
            }
            container.Add(reqFold);

            // Port
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            port.userData = resp;
            port.portName = resp.Text;
            outputContainer.Add(port);
            port.AddManipulator(ctx);

            fold.Add(container);
            foldStack.Add(fold);
            RefreshPorts();
            return foldStack;
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children().Cast<Port>())
            {
                if (!port.connected)
                {
                    continue;
                }

                _graphView.DeleteElements(port.connections);
            }
        }

        private class GraphUpdateListener : IEdgeConnectorListener
        {
            private readonly LineNode node;
            public GraphUpdateListener(LineNode node)
            {
                this.node = node;
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                Debug.Log("Graph has been changed");
                node._graphView.OnGraphChanged();
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
                node._graphView.OnGraphChanged();
            }
        }
    }
}