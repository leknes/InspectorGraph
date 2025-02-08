using Senkel;
using Senkel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public class InspectorGraphView : GraphView
    {
        private static readonly StyleSheet _gridBackgroundStyle = Resources.Load<StyleSheet>("Styles/GridBackground");

        private ComponentPortManager _portManager;
        private InspectorGraphDatabase _database;

        private GameObject _gameObject;
        private GameObjectID _id;

        public InspectorGraphView()
        {
            _portManager = new ComponentPortManager(this);
            _database = new InspectorGraphDatabase();

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            InitializeGridBackground();

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
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

        private void InitializeGridBackground()
        {
            styleSheets.Add(_gridBackgroundStyle);

            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();
        }

        private GraphViewChange SaveOnElementsMoved(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements == null)
                return graphViewChange;

            if (graphViewChange.movedElements.Count != 0)
                Save();

            return graphViewChange;
        }

        public void LoadGameObject(GameObject gameObject)
        {
            _gameObject = gameObject;
            _id = GameObjectID.From(_gameObject);

            graphViewChanged += SaveOnElementsMoved;

            InspectorGraphLoader.TryLoad(_id, out ComponentNodeLoader loader);

            LoadComponents(loader);

            EditorApplication.update += SynchronizeComponents;

            var gameObjectElement = new GameObjectElement(new SerializedObject(gameObject));

            hierarchy.Add(gameObjectElement);

            gameObjectElement.style.right = 0;
            gameObjectElement.style.top = 0;
        }

        private void LoadComponents(ComponentNodeLoader loader)
        {
            var components = _gameObject.GetComponents<Component>();
            var context = new ComponentNodeContext(_portManager);

            int defaultCount = 0;

            Rect GenerateDefaultLocalBound()
            {
                var rect = new Rect(defaultCount % 6 * (ComponentNode.InitialWidth + 32), defaultCount / 6 * 6 * 96, ComponentNode.InitialWidth, 512);

                defaultCount += 1;

                return rect;
            }

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                Type type = component.GetType();

                bool isCollapsed;
                Rect localBound;

                if (loader.TryPopNode(type.FullName, out var nodeImage))
                {
                    isCollapsed = nodeImage.IsCollapsed;
                    localBound = nodeImage.LocalBound;
                }
                else
                {
                    isCollapsed = false;
                    localBound = GenerateDefaultLocalBound();
                }

                AddComponent(context, component, localBound, isCollapsed);
            }

            context.OnComponentsInitialized();

        }

        private void AddNewComponent(ComponentNodeContext context, Component component)
        {
            bool isCollapsed;
            Rect localBound;

            if (_database.TryRecoverNode(out var image))
            {
                isCollapsed = image.IsCollapsed;
                localBound = image.LocalBound;
            }
            else
            {
                isCollapsed = false;
                localBound = new Rect(0, 0, ComponentNode.InitialWidth, 512);
            }

            AddComponent(context, component, localBound, isCollapsed);
        }

        private void SynchronizeComponents()
        {
            if (_gameObject == null)
            {
                EditorApplication.update -= SynchronizeComponents;

                return;
            }

            var components = _gameObject.GetComponents<Component>().ToList();

            var nodes = _database.Nodes.ToArray();

            foreach (var node in nodes)
            {
                if (components.Remove(node.Component))
                    continue;

                RemoveElement(node);
            }

            if (components.Count == 0)
                return;

            var context = new ComponentNodeContext(_portManager);

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                AddNewComponent(context, component);
            }

            context.OnComponentsInitialized();
        }

        private void AddComponent(ComponentNodeContext context, Component component, Rect localBound, bool isCollapsed)
        {
            var node = ComponentNode.Create(this, context, component, component.GetType(), localBound, isCollapsed);

            _database.AddNode(node);
            node.RegisterDetachFromPanelCallback(() => _database.RemoveNode(node));

            AddElement(node);

            node.RefreshExpandedState();
            node.RefreshPorts();
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

        private void Save()
        {
            var newId = GameObjectID.From(_gameObject);

            if (!_id.Equals(newId))
                InspectorGraphLoader.Delete(_id);

            _id = newId;

            InspectorGraphLoader.Save(newId, _database.ToImage());
        }

        public void Unload()
        {
            if (_gameObject == null)
                return;

            graphViewChanged -= SaveOnElementsMoved;

            EditorApplication.update -= SynchronizeComponents;

            Save();

            _database.Clear();
            _gameObject = null;
            _id = default;

            foreach (var element in graphElements)
                RemoveElement(element);

            // Removing the game object element.
            _portManager.ClearAll();
            hierarchy.RemoveAt(1);
        }
    }
}