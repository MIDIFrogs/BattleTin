using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using MIDIFrogs.FutureInThePast.Quests;
using System.Collections.Generic;

public class TriggerGraphEditor : EditorWindow
{
    private TriggerGraphView graphView;

    [MenuItem("Window/Trigger Graph Editor")]
    public static void OpenWindow()
    {
        TriggerGraphEditor window = GetWindow<TriggerGraphEditor>();
        window.titleContent = new GUIContent("Trigger Graph Editor");
    }

    private void OnEnable()
    {
        // Create GraphView
        graphView = new TriggerGraphView();
        rootVisualElement.Add(graphView);

        // Add a sample node (you can replace this with actual QuestTrigger instances)
        CompoundTrigger sampleTrigger = new();
        graphView.AddTriggerNode(sampleTrigger.RootNode);
    }
}

public class TriggerGraphView : GraphView
{
    public TriggerGraphView()
    {
        // Enable manipulation of the graph
        this.StretchToParentSize();
        this.AddManipulators();
    }

    private void AddManipulators()
    {
        // Add zooming and panning
        this.AddManipulator(new ContentZoomer());
        //this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectionListener()));
    }

    public void AddTriggerNode(CompoundTrigger.Node triggerNode)
    {
        var node = new NodeView(triggerNode);
        node.SetPosition(new Rect(100, 100, 200, 150));
        this.AddElement(node);
    }

    private class EdgeConnectionListener : IEdgeConnectorListener
    {
        public void OnDrop(GraphView graphView, Edge edge)
        {
            (edge.input.node as NodeView).node.Children.Add((edge.output.node as NodeView).node);
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> results = new();
        foreach (var port in ports)
        {
            if (startPort.direction != port.direction)
                results.Add(port);
        }
        return results;
    }
}

public class NodeView : Node
{
    public CompoundTrigger.Node node;
    private readonly Port inputPort;
    private readonly Port outputPort;

    public NodeView(CompoundTrigger.Node node)
    {
        this.node = node;

        // Add input and output ports
        inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inputPort.portName = "Input";
        inputPort.style.flexDirection = FlexDirection.RowReverse;
        inputContainer.Add(inputPort);

        outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        outputPort.portName = "Output";
        outputContainer.Add(outputPort);

        // Add a button to add children
        var addButton = new Button(() => AddChildNode()) { text = "Add Child" };
        titleContainer.Add(addButton);

        this.AddManipulator(new Dragger());
    }

    private void AddChildNode()
    {
        var child = new CompoundTrigger.Node();
        node.Children.Add(child);
        var childNode = new NodeView(child); // Assign a QuestTrigger instance here
        var edge = childNode.outputPort.ConnectTo(inputPort);
        edge.visible = true;
        Add(edge);
        childNode.SetPosition(new Rect(-100, -100, 200, 150));
        Add(childNode);
    }
}