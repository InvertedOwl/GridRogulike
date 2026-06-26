using System;
using System.Collections.Generic;
using System.Linq;
using Entities.Enemies;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GridRoguelike.EditorTools
{
    public class EnemyBrainGraphWindow : EditorWindow
    {
        private EnemyBrainGraphView graphView;
        private ObjectField brainDataField;

        [MenuItem("Tools/Enemy Brain Graph")]
        public static void Open()
        {
            EnemyBrainGraphWindow window = GetWindow<EnemyBrainGraphWindow>();
            window.titleContent = new GUIContent("Enemy Brain Graph");
        }

        private void OnEnable()
        {
            rootVisualElement.Clear();
            CreateToolbar();
            CreateGraphView();
            LoadSelectedBrainData();
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
            graphView = null;
            brainDataField = null;
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is not EnemyBrainData brainData)
                return;

            if (brainDataField != null)
                brainDataField.value = brainData;
        }

        private void CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();
            brainDataField = new ObjectField("Brain Data")
            {
                objectType = typeof(EnemyBrainData),
                allowSceneObjects = false
            };

            brainDataField.RegisterValueChangedCallback(evt =>
            {
                graphView?.SetBrainData(evt.newValue as EnemyBrainData);
            });

            Button saveButton = new Button(() =>
            {
                if (graphView?.BrainData != null)
                {
                    EditorUtility.SetDirty(graphView.BrainData);
                    AssetDatabase.SaveAssets();
                }
            })
            {
                text = "Save"
            };

            toolbar.Add(brainDataField);
            toolbar.Add(saveButton);
            rootVisualElement.Add(toolbar);
        }

        private void CreateGraphView()
        {
            graphView = new EnemyBrainGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void LoadSelectedBrainData()
        {
            if (Selection.activeObject is EnemyBrainData selectedBrainData)
                brainDataField.value = selectedBrainData;
        }
    }

    public class EnemyBrainGraphView : GraphView
    {
        private readonly Dictionary<string, EnemyBrainGraphNode> nodeViews = new();
        private bool isLoading;

        public EnemyBrainData BrainData { get; private set; }

        public EnemyBrainGraphView()
        {
            style.flexGrow = 1;
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new NoAutoPanSelectionDragger());
            this.AddManipulator(new RectangleSelector());

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            graphViewChanged = OnGraphViewChanged;
        }

        public void SetBrainData(EnemyBrainData brainData)
        {
            BrainData = brainData;
            LoadGraph();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports
                .ToList()
                .Where(endPort =>
                    endPort.direction != startPort.direction &&
                    endPort.node != startPort.node)
                .ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (BrainData == null)
            {
                evt.menu.AppendAction(
                    "Create Rule Node",
                    _ => { },
                    _ => DropdownMenuAction.Status.Disabled);
                evt.menu.AppendAction(
                    "Create Condition Node",
                    _ => { },
                    _ => DropdownMenuAction.Status.Disabled);
                return;
            }

            Vector2 graphPosition = contentViewContainer.WorldToLocal(evt.mousePosition);
            evt.menu.AppendAction("Create Rule Node", _ => CreateNode(EnemyBrainNodeType.Rule, graphPosition));
            evt.menu.AppendAction("Create Condition Node", _ => CreateNode(EnemyBrainNodeType.Condition, graphPosition));
        }

        private void LoadGraph()
        {
            isLoading = true;
            ClearGraph();

            if (BrainData == null)
            {
                isLoading = false;
                return;
            }

            Undo.RecordObject(BrainData, "Initialize Enemy Brain Graph");
            BrainData.EnsurePlanNode();
            EditorUtility.SetDirty(BrainData);

            foreach (EnemyBrainNodeData nodeData in BrainData.nodes)
            {
                EnemyBrainGraphNode node = CreateNodeView(nodeData);
                AddElement(node);
                nodeViews[nodeData.guid] = node;
            }

            foreach (EnemyBrainConnectionData connection in BrainData.connections.ToList())
            {
                if (connection == null ||
                    !nodeViews.TryGetValue(connection.fromNodeGuid, out EnemyBrainGraphNode fromNode) ||
                    !nodeViews.TryGetValue(connection.toNodeGuid, out EnemyBrainGraphNode toNode))
                {
                    continue;
                }

                Port outputPort = fromNode.GetOutputPort(connection.outputName);
                Port inputPort = toNode.InputPort;
                if (outputPort == null || inputPort == null)
                    continue;

                Edge edge = outputPort.ConnectTo(inputPort);
                AddElement(edge);
            }

            isLoading = false;
        }

        private void ClearGraph()
        {
            foreach (GraphElement element in graphElements.ToList())
            {
                RemoveElement(element);
            }

            nodeViews.Clear();
        }

        private EnemyBrainGraphNode CreateNodeView(EnemyBrainNodeData nodeData)
        {
            EnemyBrainGraphNode node = new EnemyBrainGraphNode(nodeData, MarkAssetDirty)
            {
                viewDataKey = nodeData.guid
            };

            node.SetPosition(new Rect(nodeData.editorPosition, EnemyBrainGraphNode.DefaultSize));
            return node;
        }

        private void CreateNode(EnemyBrainNodeType type, Vector2 position)
        {
            if (BrainData == null || type == EnemyBrainNodeType.Start)
                return;

            Undo.RecordObject(BrainData, $"Create {type} Node");

            EnemyBrainNodeData nodeData = new EnemyBrainNodeData
            {
                guid = Guid.NewGuid().ToString(),
                title = type == EnemyBrainNodeType.Rule ? "Rule" : "Condition",
                type = type,
                editorPosition = position
            };

            BrainData.nodes.Add(nodeData);

            EnemyBrainGraphNode node = CreateNodeView(nodeData);
            AddElement(node);
            nodeViews[nodeData.guid] = node;

            MarkAssetDirty();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (isLoading || BrainData == null)
                return change;

            if (change.elementsToRemove != null)
                change.elementsToRemove = HandleElementsToRemove(change.elementsToRemove);

            if (change.edgesToCreate != null)
                change.edgesToCreate = HandleEdgesToCreate(change.edgesToCreate);

            if (change.movedElements != null)
                HandleMovedElements(change.movedElements);

            return change;
        }

        private List<GraphElement> HandleElementsToRemove(List<GraphElement> elementsToRemove)
        {
            List<GraphElement> allowedElements = new List<GraphElement>();

            foreach (GraphElement element in elementsToRemove)
            {
                if (element is Edge edge)
                {
                    RemoveConnection(edge);
                    allowedElements.Add(element);
                    continue;
                }

                if (element is EnemyBrainGraphNode node)
                {
                    if (node.NodeData.type == EnemyBrainNodeType.Start)
                        continue;

                    RemoveNode(node);
                    allowedElements.Add(element);
                    continue;
                }

                allowedElements.Add(element);
            }

            MarkAssetDirty();
            return allowedElements;
        }

        private List<Edge> HandleEdgesToCreate(List<Edge> edges)
        {
            Undo.RecordObject(BrainData, "Connect Enemy Brain Nodes");

            List<Edge> allowedEdges = new List<Edge>();
            foreach (Edge edge in edges)
            {
                if (edge.output?.node is not EnemyBrainGraphNode fromNode ||
                    edge.input?.node is not EnemyBrainGraphNode toNode)
                {
                    continue;
                }

                string outputName = edge.output.userData as string ?? EnemyBrainData.RuleOutputName;
                bool alreadyConnected = BrainData.connections.Any(connection =>
                    connection != null &&
                    connection.fromNodeGuid == fromNode.NodeData.guid &&
                    connection.toNodeGuid == toNode.NodeData.guid &&
                    connection.outputName == outputName);

                if (alreadyConnected)
                    continue;

                allowedEdges.Add(edge);
                BrainData.connections.Add(new EnemyBrainConnectionData
                {
                    fromNodeGuid = fromNode.NodeData.guid,
                    toNodeGuid = toNode.NodeData.guid,
                    outputName = outputName
                });
            }

            MarkAssetDirty();
            return allowedEdges;
        }

        private void HandleMovedElements(List<GraphElement> movedElements)
        {
            Undo.RecordObject(BrainData, "Move Enemy Brain Node");

            foreach (GraphElement element in movedElements)
            {
                if (element is not EnemyBrainGraphNode node)
                    continue;

                node.NodeData.editorPosition = node.GetPosition().position;
            }

            MarkAssetDirty();
        }

        private void RemoveConnection(Edge edge)
        {
            if (edge.output?.node is not EnemyBrainGraphNode fromNode ||
                edge.input?.node is not EnemyBrainGraphNode toNode)
            {
                return;
            }

            Undo.RecordObject(BrainData, "Disconnect Enemy Brain Nodes");
            string outputName = edge.output.userData as string ?? EnemyBrainData.RuleOutputName;
            BrainData.connections.RemoveAll(connection =>
                connection == null ||
                connection.fromNodeGuid == fromNode.NodeData.guid &&
                connection.toNodeGuid == toNode.NodeData.guid &&
                connection.outputName == outputName);
        }

        private void RemoveNode(EnemyBrainGraphNode node)
        {
            Undo.RecordObject(BrainData, "Delete Enemy Brain Node");

            BrainData.nodes.Remove(node.NodeData);
            BrainData.connections.RemoveAll(connection =>
                connection == null ||
                connection.fromNodeGuid == node.NodeData.guid ||
                connection.toNodeGuid == node.NodeData.guid);
            nodeViews.Remove(node.NodeData.guid);
        }

        private void MarkAssetDirty()
        {
            if (isLoading || BrainData == null)
                return;

            EditorUtility.SetDirty(BrainData);
        }
    }

    public class EnemyBrainGraphNode : Node
    {
        public static readonly Vector2 DefaultSize = new Vector2(165f, 120f);

        private readonly Action markDirty;
        private readonly Dictionary<string, Port> outputPorts = new();
        private Label assetNameLabel;
        private Foldout assetValuesFoldout;
        private VisualElement assetValuesContainer;

        public EnemyBrainNodeData NodeData { get; }
        public Port InputPort { get; private set; }

        public EnemyBrainGraphNode(EnemyBrainNodeData nodeData, Action markDirty)
        {
            NodeData = nodeData;
            this.markDirty = markDirty;
            title = GetNodeTitle();
            style.width = DefaultSize.x;
            style.minWidth = DefaultSize.x;
            style.maxWidth = DefaultSize.x;

            BuildPorts();
            BuildInspectorControls();

            RefreshExpandedState();
            RefreshPorts();
        }

        public Port GetOutputPort(string outputName)
        {
            if (string.IsNullOrEmpty(outputName))
                outputName = EnemyBrainData.RuleOutputName;

            return outputPorts.TryGetValue(outputName, out Port port) ? port : null;
        }

        private string GetNodeTitle()
        {
            return NodeData.type switch
            {
                EnemyBrainNodeType.Start => EnemyBrainData.PlanNodeTitle,
                EnemyBrainNodeType.Rule => "Rule",
                EnemyBrainNodeType.Condition => "Condition",
                _ => "Node"
            };
        }

        private string GetAssetName()
        {
            return NodeData.type switch
            {
                EnemyBrainNodeType.Rule => NodeData.rule != null ? NodeData.rule.name : "Unassigned",
                EnemyBrainNodeType.Condition => NodeData.condition != null ? NodeData.condition.name : "Unassigned",
                _ => string.Empty
            };
        }

        private UnityEngine.Object GetAssignedAsset()
        {
            return NodeData.type switch
            {
                EnemyBrainNodeType.Rule => NodeData.rule,
                EnemyBrainNodeType.Condition => NodeData.condition,
                _ => null
            };
        }

        private void BuildPorts()
        {
            if (NodeData.type != EnemyBrainNodeType.Start)
            {
                InputPort = InstantiatePort(
                    Orientation.Horizontal,
                    Direction.Input,
                    Port.Capacity.Multi,
                    typeof(bool));
                InputPort.portName = "In";
                inputContainer.Add(InputPort);
            }

            switch (NodeData.type)
            {
                case EnemyBrainNodeType.Start:
                case EnemyBrainNodeType.Rule:
                    AddOutputPort(EnemyBrainData.RuleOutputName);
                    break;
                case EnemyBrainNodeType.Condition:
                    AddOutputPort(EnemyBrainData.ConditionTrueOutputName);
                    AddOutputPort(EnemyBrainData.ConditionFalseOutputName);
                    break;
            }
        }

        private void AddOutputPort(string outputName)
        {
            Port outputPort = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Multi,
                typeof(bool));
            outputPort.portName = outputName;
            outputPort.userData = outputName;
            outputContainer.Add(outputPort);
            outputPorts[outputName] = outputPort;
        }

        private void BuildInspectorControls()
        {
            if (NodeData.type == EnemyBrainNodeType.Start)
                return;

            assetNameLabel = new Label(GetAssetName());
            assetNameLabel.style.whiteSpace = WhiteSpace.Normal;
            assetNameLabel.style.marginTop = 2f;
            assetNameLabel.style.marginBottom = 3f;
            assetNameLabel.style.marginLeft = 4f;
            assetNameLabel.style.marginRight = 4f;
            assetNameLabel.style.fontSize = 11f;
            extensionContainer.Add(assetNameLabel);

            ObjectField objectField = new ObjectField
            {
                objectType = NodeData.type == EnemyBrainNodeType.Rule
                    ? typeof(EnemyBrainRule)
                    : typeof(EnemyBrainCondition),
                allowSceneObjects = false,
                value = NodeData.type == EnemyBrainNodeType.Rule
                    ? (UnityEngine.Object)NodeData.rule
                    : NodeData.condition
            };

            objectField.labelElement.style.display = DisplayStyle.None;
            objectField.style.width = Length.Percent(100f);
            objectField.style.minWidth = 0f;
            objectField.style.flexShrink = 1f;
            objectField.style.marginLeft = 0f;
            objectField.style.marginRight = 0f;

            objectField.RegisterValueChangedCallback(evt =>
            {
                if (NodeData.type == EnemyBrainNodeType.Rule)
                {
                    NodeData.rule = evt.newValue as EnemyBrainRule;
                }
                else
                {
                    NodeData.condition = evt.newValue as EnemyBrainCondition;
                }

                NodeData.title = GetAssetName();
                assetNameLabel.text = GetAssetName();
                RebuildAssetValuesInspector();
                markDirty?.Invoke();
            });

            extensionContainer.Add(objectField);

            assetValuesFoldout = new Foldout
            {
                text = "Values",
                value = false
            };
            assetValuesFoldout.style.marginTop = 4f;
            assetValuesFoldout.style.marginLeft = 0f;
            assetValuesFoldout.style.marginRight = 0f;

            assetValuesContainer = new VisualElement();
            assetValuesContainer.style.marginLeft = 0f;
            assetValuesContainer.style.marginRight = 0f;
            assetValuesContainer.style.minWidth = 0f;
            assetValuesFoldout.Add(assetValuesContainer);
            extensionContainer.Add(assetValuesFoldout);

            RebuildAssetValuesInspector();
        }

        private void RebuildAssetValuesInspector()
        {
            if (assetValuesFoldout == null || assetValuesContainer == null)
                return;

            assetValuesContainer.Clear();

            UnityEngine.Object assignedAsset = GetAssignedAsset();
            assetValuesFoldout.SetEnabled(assignedAsset != null);
            if (assignedAsset == null)
                return;

            SerializedObject serializedAsset = new SerializedObject(assignedAsset);
            serializedAsset.Update();

            SerializedProperty property = serializedAsset.GetIterator();
            bool enterChildren = true;
            bool addedAny = false;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = property.propertyType == SerializedPropertyType.Generic && !property.isArray;
                if (property.propertyPath == "m_Script")
                    continue;

                VisualElement row = CreateReadOnlyPropertyRow(property);
                if (row == null)
                    continue;

                assetValuesContainer.Add(row);
                addedAny = true;
            }

            if (!addedAny)
                assetValuesContainer.Add(CreateMutedLabel("No serialized values"));
        }

        private VisualElement CreateReadOnlyPropertyRow(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Generic && !property.isArray)
                return CreateSectionLabel(property);

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginTop = 1f;
            row.style.marginBottom = 1f;
            row.style.paddingLeft = Mathf.Min(property.depth * 8, 24);

            Label nameLabel = CreateMutedLabel($"{property.displayName}:");
            nameLabel.style.whiteSpace = WhiteSpace.NoWrap;
            nameLabel.style.flexShrink = 0f;
            nameLabel.style.marginRight = 4f;

            Label valueLabel = new Label(GetPropertyValueText(property));
            valueLabel.style.fontSize = 10f;
            valueLabel.style.unityTextAlign = TextAnchor.UpperRight;
            valueLabel.style.whiteSpace = WhiteSpace.NoWrap;
            valueLabel.style.flexGrow = 1f;
            valueLabel.style.flexBasis = 0f;
            valueLabel.style.flexShrink = 1f;

            row.Add(nameLabel);
            row.Add(valueLabel);
            return row;
        }

        private Label CreateSectionLabel(SerializedProperty property)
        {
            Label label = CreateMutedLabel(property.displayName);
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.paddingLeft = Mathf.Min(property.depth * 8, 24);
            label.style.marginTop = 2f;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            return label;
        }

        private Label CreateMutedLabel(string text)
        {
            Label label = new Label(text);
            label.style.fontSize = 10f;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.opacity = 0.8f;
            return label;
        }

        private string GetPropertyValueText(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "True" : "False";
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString("0.###");
                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(property.stringValue) ? "\"\"" : property.stringValue;
                case SerializedPropertyType.Color:
                    return $"#{ColorUtility.ToHtmlStringRGBA(property.colorValue)}";
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null ? property.objectReferenceValue.name : "None";
                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString();
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex >= 0 && property.enumValueIndex < property.enumDisplayNames.Length
                        ? property.enumDisplayNames[property.enumValueIndex]
                        : property.enumValueIndex.ToString();
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString();
                case SerializedPropertyType.ArraySize:
                    return property.intValue.ToString();
                case SerializedPropertyType.Character:
                    return ((char)property.intValue).ToString();
                case SerializedPropertyType.AnimationCurve:
                    return "Curve";
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString();
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.eulerAngles.ToString();
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue != null ? property.exposedReferenceValue.name : "None";
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize.ToString();
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString();
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString();
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceValue != null
                        ? property.managedReferenceValue.ToString()
                        : "None";
                case SerializedPropertyType.Hash128:
                    return property.hash128Value.ToString();
                default:
                    return property.isArray ? $"Array ({property.arraySize})" : property.type;
            }
        }
    }
    

public class NoAutoPanSelectionDragger : MouseManipulator
{
    private bool active;
    private Vector2 lastMousePosition;
    private readonly List<GraphElement> draggedElements = new();

    public NoAutoPanSelectionDragger()
    {
        activators.Add(new ManipulatorActivationFilter
        {
            button = MouseButton.LeftMouse
        });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (active)
            return;

        if (!CanStartManipulation(evt))
            return;

        if (evt.target is not GraphElement clickedElement)
            return;

        GraphView graphView = target as GraphView;
        if (graphView == null)
            return;

        draggedElements.Clear();

        foreach (ISelectable selectable in graphView.selection)
        {
            if (selectable is GraphElement graphElement && graphElement.IsMovable())
                draggedElements.Add(graphElement);
        }

        if (!draggedElements.Contains(clickedElement) && clickedElement.IsMovable())
        {
            graphView.ClearSelection();
            graphView.AddToSelection(clickedElement);
            draggedElements.Add(clickedElement);
        }

        if (draggedElements.Count == 0)
            return;

        active = true;
        lastMousePosition = evt.localMousePosition;

        target.CaptureMouse();
        evt.StopImmediatePropagation();
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (!active || !target.HasMouseCapture())
            return;

        Vector2 delta = evt.localMousePosition - lastMousePosition;
        lastMousePosition = evt.localMousePosition;

        foreach (GraphElement element in draggedElements.ToList())
        {
            Rect position = element.GetPosition();
            position.position += delta;
            element.SetPosition(position);
        }

        evt.StopImmediatePropagation();
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (!active || !CanStopManipulation(evt))
            return;

        StopDragging(evt);
    }

    private void OnMouseCaptureOut(MouseCaptureOutEvent evt)
    {
        active = false;
        draggedElements.Clear();
    }

    private void StopDragging(EventBase evt)
    {
        active = false;
        draggedElements.Clear();

        if (target.HasMouseCapture())
            target.ReleaseMouse();

        evt.StopImmediatePropagation();
    }
}
}
