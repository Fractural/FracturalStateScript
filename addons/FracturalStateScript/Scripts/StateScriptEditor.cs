using Fractural.Plugin;
using Fractural.Plugin.AssetsRegistry;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tests;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    // TODO: Test this
    // TODO: Make StateScriptEditor listen to scene tree changes (renames, deletions, additions)
    //       in order to keep graph up to date.
    [Tool]
    public class StateScriptEditor : Control
    {
        private class CSharpData
        {
            public IDictionary<IAction, IList<StateNodeConnection>> StateToConnectionsDict { get; set; }
            public IDictionary<IAction, StateScriptGraphNode> StateToGraphNodeDict { get; set; }
            public IStateGraph StateGraph { get; set; }
            public IRawStateGraph RawStateGraph => StateGraph as IRawStateGraph;
            public Node StateGraphNode => StateGraph as Node;
            public Type[] StateTypes { get; set; }
        }

        private GraphEdit _graphEdit;
        private Label _variableLabel;

        private Button _createStateButton;
        private ColorRect _popupOverlayRect;
        private SearchDialog _stateSearchDialog;
        private IAssetsRegistry _assetsRegistry;
        private EditorPlugin _plugin;

        private bool _canSave = true;

        private CSharpData _data;

        public StateScriptEditor() { }
        public StateScriptEditor(EditorPlugin plugin, IAssetsRegistry assetsRegistry)
        {
            _data = new CSharpData();
            RectMinSize = new Vector2(0, 200 * assetsRegistry.Scale);

            _plugin = plugin;
            _assetsRegistry = assetsRegistry;

            _graphEdit = new GraphEdit();
            AddChild(_graphEdit);
            _graphEdit.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
            _graphEdit.Connect("delete_nodes_request", this, nameof(OnGraphEditDeleteNodesRequest));
            _graphEdit.Connect("disconnection_request", this, nameof(OnGraphEditDisconnectionRequest));
            _graphEdit.Connect("connection_request", this, nameof(OnGraphEditConnectionRequest));
            _graphEdit.Connect("_end_node_move", this, nameof(OnGraphEditEndNodeMove));
            _graphEdit.RightDisconnects = true;

            var marginContainer = new MarginContainer();
            int margin = (int)(16 * _assetsRegistry.Scale);
            marginContainer.AddConstantOverride("margin_right", margin);
            marginContainer.AddConstantOverride("margin_top", margin);
            marginContainer.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(marginContainer);
            marginContainer.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);

            _variableLabel = new Label();
            _variableLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _variableLabel.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
            _variableLabel.MouseFilter = MouseFilterEnum.Ignore;

            marginContainer.AddChild(_variableLabel);

            _data.StateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where((x) =>
                    !x.IsAbstract &&
                    x.GetCustomAttribute<CSharpScriptAttribute>() != null &&
                    typeof(IAction).IsAssignableFrom(x) &&
                    typeof(Node).IsAssignableFrom(x) &&
                    x != typeof(StateGraph))
                .ToArray();

            _popupOverlayRect = new ColorRect();
            _popupOverlayRect.Color = new Color(Colors.Black, 0.5f);
            _popupOverlayRect.Visible = false;
            AddChild(_popupOverlayRect);
            _popupOverlayRect.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);

            _stateSearchDialog = new SearchDialog();
            _stateSearchDialog.Connect(nameof(SearchDialog.EntrySelected), this, nameof(OnStateSearchEntrySelected));
            _stateSearchDialog.Connect("popup_hide", this, nameof(OnStateSearchDialogHide));
            _stateSearchDialog.SearchEntries = _data.StateTypes.Select(x => new SearchEntry()
            {
                Text = x.FullName
            }).ToArray();
            AddChild(_stateSearchDialog);

            _createStateButton = new Button();
            _createStateButton.Text = "Create State";
            _createStateButton.Shortcut = new ShortCut()
            {
                Shortcut = new InputEventKey()
                {
                    Control = true,
                    PhysicalScancode = (int)KeyList.B,
                }
            };
            _createStateButton.Connect("pressed", this, nameof(OnCreateStateButtonPressed));
            _graphEdit.GetZoomHbox().AddChild(_createStateButton);

            GD.Print("Constructed");
        }

        public override void _Ready()
        {
            if (NodeUtils.IsInEditorSceneTab(this)) return;

            GD.Print("Instanced!");
        }

        public void Load(IStateGraph stateGraph)
        {
            _data.StateGraph = stateGraph;
            if (!(stateGraph is IRawStateGraph rawGraph) || !(stateGraph is Node stateGraphNode)) return;
            var result = StateScriptUtils.ConnectionListDictFromGDDict(stateGraphNode, rawGraph.RawConnections);
            bool needsResave = result.HadParseFailures;
            _data.StateToConnectionsDict = result.Dictionary;
            _data.StateToGraphNodeDict = new Dictionary<IAction, StateScriptGraphNode>();
            GD.Print("Loaded state graph, ", (stateGraph as Node).GetPath());

            _graphEdit.ClearConnections();
            foreach (Node child in _graphEdit.GetChildren())
            {
                if (child is StateScriptGraphNode)
                    child.QueueFree();
            }

            var uselessStates = new List<string>();
            if (_data.RawStateGraph.StateNodePositions == null)
                _data.RawStateGraph.StateNodePositions = new GDC.Dictionary();
            foreach (string state in _data.RawStateGraph.StateNodePositions.Keys)
            {
                if (!_data.StateGraphNode.HasNode(state))
                    uselessStates.Add(state);
            }
            if (uselessStates.Count > 0)
                needsResave = true;
            foreach (string uselessState in uselessStates)
                _data.RawStateGraph.StateNodePositions.Remove(uselessState);

            foreach (Node child in stateGraphNode.GetChildren())
            {
                if (child is IAction state)
                {
                    var graphNode = CreateGraphNode(state);
                    _graphEdit.AddChild(graphNode);
                    graphNode.Offset = _data.RawStateGraph.StateNodePositions.Get<Vector2>(child.Name);
                    _data.StateToGraphNodeDict[state] = graphNode;
                }
            }

            foreach (var pair in _data.StateToConnectionsDict)
            {
                var fromState = pair.Key;
                var connections = pair.Value;
                foreach (var connection in connections)
                {
                    var fromStateGraphNode = _data.StateToGraphNodeDict[fromState];
                    var toStateGraphNode = _data.StateToGraphNodeDict[connection.ToState];
                    _graphEdit.ConnectNode(fromStateGraphNode.Name, fromStateGraphNode.GetOutputPortFromEvent(connection.FromEvent), toStateGraphNode.Name, toStateGraphNode.GetInputPortFromMethod(connection.ToMethod));
                }
            }

            GetTree().Connect("node_added", this, nameof(OnNodeAdded));
            GetTree().Connect("node_removed", this, nameof(OnNodeRemoved));

            if (needsResave)
                Save();
        }

        public void Unload()
        {
            GetTree().Disconnect("node_added", this, nameof(OnNodeAdded));
            GetTree().Disconnect("node_removed", this, nameof(OnNodeRemoved));
            GD.Print("Unloaded!");
        }

        private StateScriptGraphNode CreateGraphNode(IAction state)
        {
            StateScriptGraphNode node;
            if (state is Entry)
            {
                node = new EntryGraphNode();
            }
            else if (state is Exit)
            {
                node = new ExitGraphNode();
            }
            else if (state is IState)
            {
                node = new StateGraphNode();
            }
            else
            {
                node = new ActionGraphNode();
            }
            node.State = state;
            node.Offset = _graphEdit.ScrollOffset + (_graphEdit.RectSize / 2) - node.RectSize;
            node.Connect("close_request", this, nameof(OnGraphEditCloseNodeRequest), GDUtils.GDParams(node));
            return node;
        }

        private void Save()
        {
            if (!_canSave) return;
            _data.RawStateGraph.StateNodePositions = new GDC.Dictionary();
            foreach (Node child in _graphEdit.GetChildren())
            {
                if (child is StateScriptGraphNode graphNode)
                    _data.RawStateGraph.StateNodePositions[(graphNode.State as Node).Name] = graphNode.Offset;
            }
            _data.RawStateGraph.RawConnections = StateScriptUtils.ConnectionListDictToGDDict(_data.StateToConnectionsDict);
        }

        private void OnStateSearchDialogHide()
        {
            _popupOverlayRect.Visible = false;
        }

        private void OnCreateStateButtonPressed()
        {
            _popupOverlayRect.Visible = true;
            var globalRectSize = GetGlobalRect().Size;
            var smallestSize = globalRectSize.x > globalRectSize.y ? globalRectSize.y : globalRectSize.x;
            _stateSearchDialog.Popup_(GetGlobalRect().AddPadding(-smallestSize * 0.25f * _assetsRegistry.Scale));
        }

        private void OnStateSearchEntrySelected(string stateTypeName)
        {
            var stateType = _data.StateTypes.FirstOrDefault(x => x.FullName == stateTypeName);
            var csharpScript = GD.Load<CSharpScript>(stateType.GetCustomAttribute<CSharpScriptAttribute>().FilePath);
            var stateInstance = csharpScript.New() as Node;
            _data.StateGraphNode.AddChild(stateInstance);
            stateInstance.Owner = _plugin.GetEditorInterface().GetEditedSceneRoot();
        }

        private void OnNodeAdded(Node node)
        {
            if (node.GetParent() == _data.StateGraphNode && node is IAction state)
            {
                var stateScriptNode = CreateGraphNode(state);
                _graphEdit.AddChild(stateScriptNode);
            }
        }

        private void OnNodeRemoved(Node node)
        {
            if (node.GetParent() == _data.StateGraphNode && node is IAction state)
            {
                _data.StateToConnectionsDict.Remove(state);
                foreach (var connections in _data.StateToConnectionsDict.Values)
                {
                    for (int i = connections.Count - 1; i >= 0; i--)
                        if (connections[i].ToState == state)
                            connections.RemoveAt(i);
                }
            }
        }

        private void OnGraphEditEndNodeMove()
        {
            Save();
        }

        private void OnGraphEditConnectionRequest(string from, int fromSlot, string to, int toSlot)
        {
            var fromNode = _graphEdit.GetNode(from) as StateScriptGraphNode;
            var toNode = _graphEdit.GetNode(to) as StateScriptGraphNode;
            if (fromNode == null || toNode == null) return;

            if (!_graphEdit.IsNodeConnected(from, fromSlot, to, toSlot) &&
                _graphEdit.ConnectNode(from, fromSlot, to, toSlot) == Error.Ok)
            {
                var fromEvent = fromNode.GetOutputPortEvent(fromSlot);
                var toMethod = toNode.GetInputPortMethod(toSlot);

                if (!_data.StateToConnectionsDict.ContainsKey(fromNode.State))
                    _data.StateToConnectionsDict[fromNode.State] = new List<StateNodeConnection>();
                _data.StateToConnectionsDict[fromNode.State].Add(new StateNodeConnection()
                {
                    FromEvent = fromEvent,
                    ToMethod = toMethod,
                    ToState = toNode.State
                });

                Save();
            }
            Save();
        }

        private void OnGraphEditDisconnectionRequest(string from, int fromSlot, string to, int toSlot)
        {
            var fromNode = _graphEdit.GetNode(from) as StateScriptGraphNode;
            var toNode = _graphEdit.GetNode(to) as StateScriptGraphNode;
            if (fromNode == null || toNode == null) return;

            if (_graphEdit.IsNodeConnected(from, fromSlot, to, toSlot))
            {
                _graphEdit.DisconnectNode(from, fromSlot, to, toSlot);

                var fromEvent = fromNode.GetOutputPortEvent(fromSlot);
                var toMethod = toNode.GetInputPortMethod(toSlot);

                if (!_data.StateToConnectionsDict.ContainsKey(fromNode.State))
                    return;

                var connectionsArray = _data.StateToConnectionsDict[fromNode.State];
                for (int i = 0; i < connectionsArray.Count; i++)
                {
                    var connection = connectionsArray[i];
                    if (connection.FromEvent == fromEvent && connection.ToState == toNode && connection.ToMethod == toMethod)
                    {
                        connectionsArray.RemoveAt(i);
                        break;
                    }
                }
            }
            Save();
        }

        private void OnGraphEditCloseNodeRequest(Node node)
        {
            OnGraphEditDeleteNodesRequest(new GDC.Array() { node.Name });
        }

        private void OnGraphEditDeleteNodesRequest(GDC.Array nodes)
        {
            _canSave = false;
            HashSet<string> deletedNodes = new HashSet<string>();
            foreach (string nodeName in nodes)
            {
                var node = _graphEdit.GetNodeOrNull(nodeName);
                if (node == null)
                    continue;
                if (node is StateScriptGraphNode graphNode)
                    (graphNode.State as Node).QueueFree();
                deletedNodes.Add(node.Name);
                node.QueueFree();
            }
            foreach (GDC.Dictionary connection in _graphEdit.GetConnectionList())
            {
                if (deletedNodes.Contains(connection.Get<string>("from")) || deletedNodes.Contains(connection.Get<string>("to")))
                    OnGraphEditDisconnectionRequest(connection.Get<string>("from"), connection.Get<int>("from_port"), connection.Get<string>("to"), connection.Get<int>("to_port"));
            }
            _canSave = true;
            Save();
        }
    }
}
