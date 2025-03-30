using InspectorGraph;
using NUnit.Framework;
using Senkel;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace InspectorGraph
{
    public class InspectorGraphView : GraphView
    {
        private static readonly StyleSheet _gridBackgroundStyle = Resources.Load<StyleSheet>("Styles/GridBackground");

        private delegate ComponentNodeData ComponentNodeDataProvider(Component component);

        private readonly InspectorGraphWindow _window;

        private readonly InspectorGraphRegistry _registry;

        private readonly InspectorGraphManager _manager;

        private readonly ComponentNodeFactory _factory;

        private GameObject _currentGameObject;

        private bool _loaded;

        public InspectorGraphView(InspectorGraphWindow window)
        {
            _window = window;
            _manager = new InspectorGraphManager(this);
            _registry = new InspectorGraphRegistry();
            _factory = new ComponentNodeFactory(_manager);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            InitializeGridBackground();

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }
      

        private void InitializeGridBackground()
        {
            styleSheets.Add(_gridBackgroundStyle);

            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();
        }


        private GameObjectData ToData()
        {
            var nodes = _registry.Nodes;

            ComponentNodeMetadata[] components = new ComponentNodeMetadata[nodes.Count];

            for(int i = 0; i < nodes.Count; i++)
                components[i] = nodes[i].ToMetadata();

            return new GameObjectData { Components = components };
        }

        private ComponentNodeDataProvider CreateDataProvider()
        {
            DefaultComponentDataFactory factory = new DefaultComponentDataFactory();
             
            return component => factory.Create();
        }

        private ComponentNodeDataProvider CreateDataProvider(GameObjectData data)
        {
            int count = data.Components.Length;

            List<ComponentNodeMetadata> components = new List<ComponentNodeMetadata>(count);

            for (int i = count - 1; i >= 0; i--)
                components.Add(data.Components[i]);

            DefaultComponentDataFactory factory = new DefaultComponentDataFactory();

            return component =>
            {
                count = components.Count;

                for (int i = count - 1; i >= 0; i--)
                {
                    ComponentNodeMetadata metadata = components[i];

                    if (metadata.Name != component.GetType().Name)
                        continue;

                    components.RemoveAt(i);

                    return metadata.Data;
                }

                return factory.Create();
            };
        }

        private void BindGameObject()
        {
            // This has to load game object data, load nodes and be responsible for unbinding again.
            // * Load all nodes using the InspectorGraphDatabase.

            EditorApplication.update += CheckForComponentChanges;

            graphViewChanged += SaveOnElementsMoved;

            ComponentNodeDataProvider provider;

            if (InspectorGraphSelection.TryLoadGameObject(out GameObjectData data))
                provider = CreateDataProvider(data);
            else
                provider = CreateDataProvider();
             
            List<Component> componentList = _currentGameObject.GetComponentsNoNull();

            ComponentNodeInfo[] infoArray = new ComponentNodeInfo[componentList.Count];

            for (int i = 0; i < componentList.Count; i++)
            {
                Component component = componentList[i];

                infoArray[i] = new ComponentNodeInfo
                {
                    Component = component,
                    Data = provider(component)
                };
            }
             
            var nodeArray = _factory.Create(infoArray);

            for (int i = 0; i < nodeArray.Length; i++)
                BindComponent(nodeArray[i]);
             
            var gameObjectElement = new GameObjectElement(new SerializedObject(_currentGameObject));

            hierarchy.Add(gameObjectElement);

            gameObjectElement.style.right = 0;
            gameObjectElement.style.top = 0;
        }
         
        private void BindComponent(ComponentNode node)
        { 
            _registry.RegisterNode(node);
            node.RegisterDetachFromPanelCallback(() => _registry.UnregisterNode(node));

            AddElement(node);

            node.RefreshExpandedState();
            node.RefreshPorts();
        }
         
        private void CheckForComponentChanges()
        {  
            if (_currentGameObject == null)
            {
                EditorApplication.update -= CheckForComponentChanges;

                return;
            }
              
            var componentList = _currentGameObject.GetComponents<Component>().ToList();

            var nodeArray = _registry.Nodes.ToArray();

            // This is rather clunky fix, prohibiting the duplication of nodes, when refocusing the inspector graph.
            
            if (nodeArray.Length == 0)
                return;

            bool changed = false;

            foreach (var node in nodeArray)
            {
                if (componentList.Remove(node.Component))
                    continue;

                RemoveElement(node);

                changed = true;
            }

            if (componentList.Count == 0)
                return;
              
            List<ComponentNodeInfo> infoList = new List<ComponentNodeInfo>(componentList.Count);

            DefaultComponentDataFactory factory = new DefaultComponentDataFactory();

            foreach (var component in componentList)
            {
                if (component == null)
                    continue;

                ComponentNodeData data;

                if (!_registry.TryRecoverNode(out data))
                    data = factory.Create();

                infoList.Add(new ComponentNodeInfo
                {
                    Component = component,
                    Data = data
                });
            }

            int prevArrayLen = nodeArray.Length;

            nodeArray = _factory.Create(infoList.ToArray());

            for (int i = 0; i < nodeArray.Length; i++)
            { 
                BindComponent(nodeArray[i]);

                changed = true;
            }

            if(changed)
                InspectorGraphSelection.SaveGameObject(ToData());
        }

        private GraphViewChange SaveOnElementsMoved(GraphViewChange graphViewChange)
        {  
            if (graphViewChange.movedElements == null)
                return graphViewChange;

            if (graphViewChange.movedElements.Count != 0)
                InspectorGraphSelection.SaveGameObject(ToData());

            return graphViewChange;
        }
          
        private void UnbindGameObject()
        {
            graphViewChanged -= SaveOnElementsMoved;

            EditorApplication.update -= CheckForComponentChanges;

            InspectorGraphDatabase.Save(GameObjectIdentfier.From(_currentGameObject), ToData());

            _registry.Clear();
            _currentGameObject = null;

            foreach (var element in graphElements)
                RemoveElement(element);

            // Removing the game object element.
            _manager.ClearAll();
            hierarchy.RemoveAt(1);
        }
         
        private void OnSelectionChanged()
        {
            TryUnbindGameObject();

            TryBindGameObject();
        }

        private void TryUnbindGameObject()
        {
            if (_currentGameObject == null)
                return;
             
            UnbindGameObject();
        }

        private void TryBindGameObject()
        {
            _currentGameObject = InspectorGraphSelection.CurrentGameObject;

            if (_currentGameObject != null)
                BindGameObject();
        }

        public void Load()
        {
            if (_loaded)
                return;

            _loaded = true;

            TryBindGameObject();

            InspectorGraphSelection.SelectionChanged += OnSelectionChanged;
        }
         
        public void Unload()
        {
            if (!_loaded)
                return;

            _loaded = false;

            TryUnbindGameObject();

            InspectorGraphSelection.SelectionChanged -= OnSelectionChanged;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is Edge)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Delete", delegate
                {
                    DeleteSelectionCallback(AskUser.DontAskUser);
                }, (a) => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>(8);

            foreach (var port in ports)
            {
                if (startPort.node == port.node)
                    continue;

                if (startPort.direction == port.direction)
                    continue;

                bool isCompatible = false;

                switch (startPort.direction)
                {
                    case Direction.Input:
                        isCompatible = startPort.portType.IsAssignableFrom(port.portType);
                        break;

                    case Direction.Output:
                        isCompatible = startPort.portType.IsAssignableTo(port.portType);
                        break;
                }

                if (isCompatible)
                    compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        private class DefaultComponentDataFactory
        {
            private int _version;

            public int XOffset = 32;
            public int YOffset = 96;

            public int RowLength = 6;
             
            private Rect CreateLocalBound()
            {
                return new Rect(_version % RowLength * (ComponentNode.InitialWidth + XOffset), _version / RowLength * RowLength * 96, ComponentNode.InitialWidth, 512);
            }

            public ComponentNodeData Create()
            {
                ComponentNodeData data = new ComponentNodeData
                {
                    Collapsed = false,
                    LocalBound = CreateLocalBound(),
                };

                _version++;

                return data;
            }
        }
    }
}
