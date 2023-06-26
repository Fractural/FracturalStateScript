﻿using Fractural.Plugin;
using Fractural.Plugin.AssetsRegistry;
using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GDC = Godot.Collections;

namespace Fractural.StateScript
{
    // TODO: Test this
    // TODO: Make StateScriptEditor listen to scene tree changes (renames, deletions, additions)
    //       in order to keep graph up to date.
    [Tool]
    public class StateScriptEditor : Control
    {
        private GraphEdit _graphEdit;
        private Label _variableLabel;

        private Button _createStateButton;
        private ColorRect _popupOverlayRect;
        private SearchDialog _stateSearchDialog;
        private IAssetsRegistry _assetsRegistry;

        public IStateGraph StateGraph { get; private set; }
        public IRawStateGraph RawStateGraph => StateGraph as IRawStateGraph;
        public Node StateGraphNode => StateGraph as Node;
        public IDictionary<IAction, IList<StateNodeConnection>> StateToConnectionsDict { get; private set; }
        public IDictionary<IAction, StateScriptGraphNode> StateToGraphNodeDict { get; private set; }

        public Type[] StateTypes { get; private set; }

        public StateScriptEditor() { }
        public StateScriptEditor(IAssetsRegistry assetsRegistry)
        {
            RectMinSize = new Vector2(0, 200 * assetsRegistry.Scale);

            _assetsRegistry = assetsRegistry;

            _graphEdit = new GraphEdit();
            AddChild(_graphEdit);
            _graphEdit.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
            _graphEdit.Connect("connection_request", this, nameof(OnGraphEditConnectionRequest));

            var marginContainer = new MarginContainer();
            int margin = (int)(16 * _assetsRegistry.Scale);
            marginContainer.AddConstantOverride("margin_right", margin);
            marginContainer.AddConstantOverride("margin_top", margin);
            marginContainer.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(marginContainer);
            marginContainer.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);

            _variableLabel = new Label();
            _variableLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            _variableLabel.SizeFlagsVertical = (int)SizeFlags.ExpandFill; ;
            _variableLabel.MouseFilter = MouseFilterEnum.Ignore;

            marginContainer.AddChild(_variableLabel);

            StateTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && x.IsAssignableFrom(typeof(IAction)) && x.IsAssignableFrom(typeof(Node)) && x != typeof(StateGraph)).ToArray();

            _popupOverlayRect = new ColorRect();
            _popupOverlayRect.Color = new Color(Colors.Black, 0.5f);
            _popupOverlayRect.Visible = false;
            AddChild(_popupOverlayRect);
            _popupOverlayRect.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);

            _stateSearchDialog = new SearchDialog();
            _stateSearchDialog.Connect(nameof(SearchDialog.EntrySelected), this, nameof(OnStateSearchEntrySelected));
            _stateSearchDialog.Connect("popup_hide", this, nameof(OnStateSearchDialogHide));
            _stateSearchDialog.SearchEntries = StateTypes.Select(x => new SearchEntry()
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
            StateGraph = stateGraph;
            if (!(stateGraph is IRawStateGraph rawGraph) || !(stateGraph is Node stateGraphNode)) return;
            StateToConnectionsDict = StateScriptUtils.ConnectionListDictFromGDDict(stateGraphNode, rawGraph.RawConnections);
            StateToGraphNodeDict = new Dictionary<IAction, StateScriptGraphNode>();
            GD.Print("Loaded state graph, ", (stateGraph as Node).GetPath());

            foreach (Node child in _graphEdit.GetChildren())
            {
                if (child is StateScriptGraphNode)
                    child.QueueFree();
            }

            var uselessStates = new List<string>();
            if (RawStateGraph.StateNodePositions == null)
                RawStateGraph.StateNodePositions = new GDC.Dictionary();
            foreach (string state in RawStateGraph.StateNodePositions.Keys)
            {
                if (!StateGraphNode.HasNode(state))
                    uselessStates.PushBack(state);
            }
            foreach (string uselessState in uselessStates)
                RawStateGraph.StateNodePositions.Remove(uselessState);

            foreach (Node child in stateGraphNode.GetChildren())
            {
                if (child is IAction state)
                {
                    var node = CreateGraphNode(state);
                    _graphEdit.AddChild(node);
                    node.RectPosition = RawStateGraph.StateNodePositions.Get<Vector2>(child.Name);
                    StateToGraphNodeDict[state] = node;
                }
            }

            bool connectionLoadFailed = false;
            foreach (var pair in StateToConnectionsDict)
            {
                var state = pair.Key;
                var connections = pair.Value;
                foreach (var connection in connections)
                {
                    if (!StateToGraphNodeDict.TryGetValue(state, out var fromState) ||
                        !StateToGraphNodeDict.TryGetValue(connection.ToState, out var toState))
                    {
                        connectionLoadFailed = true;
                        continue;
                    }
                    _graphEdit.ConnectNode(fromState.Name, fromState.GetOutputPortFromEvent(connection.FromEvent), toState.Name, fromState.GetInputPortFromMethod(connection.ToMethod));
                }
            }

            GetTree().Connect("node_added", this, nameof(OnNodeAdded));
            GetTree().Connect("node_removed", this, nameof(OnNodeRemoved));

            if (connectionLoadFailed)
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
            return node;
        }

        private void Save()
        {
            RawStateGraph.StateNodePositions = new GDC.Dictionary();
            foreach (Node child in _graphEdit.GetChildren())
            {
                if (child is StateScriptGraphNode graphNode)
                    RawStateGraph.StateNodePositions[child.Name] = graphNode.RectPosition;
            }
            RawStateGraph.RawConnections = StateScriptUtils.ConnectionListDictToGDDict(StateGraphNode, StateToConnectionsDict);
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
            var stateType = StateTypes.FirstOrDefault(x => x.FullName == stateTypeName);
            var stateInstance = Activator.CreateInstance(stateType) as Node;
            _graphEdit.AddChild(stateInstance);
        }

        private void OnNodeAdded(Node node)
        {
            if (node.GetParent() == StateGraphNode && node is IAction state)
            {
                var stateScriptNode = CreateGraphNode(state);
                _graphEdit.AddChild(stateScriptNode);
            }
        }

        private void OnNodeRemoved(Node node)
        {
            if (node.GetParent() == StateGraphNode && node is IAction state)
            {
                StateToConnectionsDict.Remove(state);
                foreach (var connections in StateToConnectionsDict.Values)
                {
                    for (int i = connections.Count - 1; i >= 0; i--)
                        if (connections[i].ToState == state)
                            connections.RemoveAt(i);
                }
            }
        }

        private void OnGraphEditConnectionRequest(string from, int from_slot, string to, int to_slot)
        {
            var fromNode = _graphEdit.GetNode(from) as StateScriptGraphNode;
            var toNode = _graphEdit.GetNode(to) as StateScriptGraphNode;
            if (fromNode == null || toNode == null) return;

            if (!_graphEdit.IsNodeConnected(from, from_slot, to, to_slot) &&
                _graphEdit.ConnectNode(from, from_slot, to, to_slot) == Error.Ok)
            {
                var fromEvent = fromNode.GetOutputPortEvent(from_slot);
                var toMethod = toNode.GetInputPortMethod(to_slot);

                if (!StateToConnectionsDict.ContainsKey(fromNode.State))
                    StateToConnectionsDict[fromNode.State] = new List<StateNodeConnection>();
                StateToConnectionsDict[fromNode.State].Add(new StateNodeConnection()
                {
                    FromEvent = fromEvent,
                    ToMethod = toMethod,
                    ToState = toNode.State
                });

                Save();
            }
        }
    }
}
