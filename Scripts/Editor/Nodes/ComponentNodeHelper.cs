using Senkel.Reflection; 
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Senkel;
using Leknes.InterfaceSerialization;

namespace InspectorGraph
{
    public class ComponentNodeHelper
    {
        private static readonly StyleSheet _nodeStyle = Resources.Load<StyleSheet>("Styles/Node");

        private static readonly StyleSheet _inspectorElementStyle = Resources.Load<StyleSheet>("Styles/InspectorElement");

        private const float _initialMinWidth = 384;

        private readonly InspectorGraphManager _manager;

        private readonly SerializedObject _object;

        private readonly ComponentNode _node;

        private VisualElement _inspectorContainer;

        private VisualElement _newTitleContainer;

        private readonly Type _componentType;

        public Action Connect { get; private set; }



        public ComponentNodeHelper(InspectorGraphManager manager, ComponentNode node)
        {
            _manager = manager;

            _object = new SerializedObject(node.Component);

            _node = node;

            _componentType = node.Component.GetType();
        }

        public void SetInitialState()
        {
            _node.styleSheets.Add(_nodeStyle);

            _node.style.minWidth = _initialMinWidth;

            _node.title = DisplayName.TypeName(_node.Component.GetType());
        }

        public void AddTitleContainer()
        {
            _newTitleContainer = new VisualElement() { name = "new-title" };
             
            _newTitleContainer.style.flexDirection = FlexDirection.Row;

            _node.mainContainer.Insert(0, _newTitleContainer);
            _node.mainContainer.Remove(_node.titleContainer);

            _newTitleContainer.Add(_node.titleContainer);

            _node.titleContainer.Insert(0, _node.titleButtonContainer);
            _node.titleButtonContainer.RemoveAt(0);
        }

        public void AddToggle()
        {
            SerializedProperty enabled = _object.FindProperty("m_Enabled");

            if (enabled == null)
                return;

            var toggle = new Toggle();

            toggle.BindProperty(enabled);

            _node.titleContainer.Insert(_node.titleContainer.childCount - 1, toggle);
        }

        public void AddIcon()
        {
            Texture icon = null;

            UnityLogger.InvokeWithLogDisabled(() => icon = EditorGUIUtility.IconContent($"{_componentType.Name} Icon").image);

            if (icon == null)
            {
                if (_node.Component is not MonoBehaviour)
                    return;

                if (_componentType.Namespace != null && (_componentType.Namespace.StartsWith("UnityEngine") || _componentType.Namespace.StartsWith("UnityEditor")))
                    return;

                icon = EditorGUIUtility.IconContent($"cs Script Icon").image;
            }

            _node.titleContainer.Insert(1, new Image() { image = icon });
        }

        public void AddOutput()
        {
            Port output = _node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, _componentType);

            output.portName = string.Empty;

            var newOutputContainer = new VisualElement() { name = "new-output" };

            _newTitleContainer.Add(newOutputContainer);
            newOutputContainer.Add(output);

            _manager.AddOutput(output, _node.Component);
        }

        private void FixIMGUIContainer(IMGUIContainer imguiContainer)
        {
            // There is a rare unexpected behaviour that when the width of a node is resized, the height of the IMGUI container changes,
            // without reflecting this change in the resolved style or style at all.

            // I guess this does not require manual garbage collection, because surely this event won't fire when the `IMGUIContainer` is removed.

            imguiContainer.onGUIHandler += () =>
            {
                float previousHeigth = imguiContainer.resolvedStyle.height;
                float newHeight = GUILayoutUtility.GetLastRect().height;

                if (newHeight == 1 || previousHeigth == 1 || float.IsNaN(previousHeigth) || previousHeigth == newHeight)
                    return;

                imguiContainer.style.height = newHeight;

                imguiContainer.MarkDirtyRepaint();
            };
        }

        private void FixListView()
        {
            static bool IsListView(VisualElement target)
            {
                if (target == null)
                    return false;

                bool isListView = target.name == "unity-list-view__reorderable-item";
                isListView |= target.name == "unity-list-view__reorderable-handle";
                isListView |= target.name == "unity-list-view__reorderable-handle-bar";
                isListView |= target.name == "unity-list-view__reorderable-item__container";

                if (isListView || target is not Label)
                    return isListView || target.parent.name == "unity-list-view__reorderable-item__container";

                return target.parent.parent.parent.name == "unity-list-view__reorderable-item__container";
            }

            _node.RegisterCallback<PointerDownEvent>(pointerDownEvent =>
            {
                if (IsListView((VisualElement)pointerDownEvent.target))
                    pointerDownEvent.StopPropagation();
            });
        }

        public void AddInspectorElement()
        { 
            var inspectorElement = new InspectorElement(_object);

            if (inspectorElement.IsEmpty())
                return;

            _inspectorContainer = new VisualElement() { name = "inspector" };
             
            _node.Add(_inspectorContainer);

            _inspectorContainer.Add(inspectorElement);

            _node.styleSheets.Add(_inspectorElementStyle);

            var imguiContainer = inspectorElement.GetIMGUIContainer();

            if(imguiContainer != null)
                FixIMGUIContainer(imguiContainer);
            else
                FixListView();
        }

        public void AddInput()
        {
            string GetDisplayName(SerializedProperty property, Type propertyType)
            {
                return $"{property.displayName} ({DisplayName.TypeName(propertyType)})";
            }

            void AddListViewInput(SerializedProperty property)
            {
                FieldInfo field = property.GetField();

                if (field == null)
                    return;

                Type propertyType = field.FieldType;

                Type elementType;
                 
                if (propertyType.IsArray)
                    elementType = propertyType.GetElementType();
                else
                {
                    if (propertyType == typeof(string))
                        return;

                    elementType = propertyType.GetFirstGenericArgument();
                }

                Func<SerializedProperty, SerializedProperty> elementGetter;

                if (elementType.HasGenericTypeDefinition(typeof(Interface<>)))
                {
                    elementType = elementType.GetFirstGenericArgument();
                    elementGetter = elementProperty => elementProperty.FindPropertyRelative("_underlyingValue");
                }
                else
                {
                    elementGetter = elementProperty => elementProperty;

                    if (field.TryGetCustomAttribute(out InterfaceAttribute interfaceAttribute))
                        elementType = interfaceAttribute.Type;
                }

                if (!elementType.IsInterface && !elementType.IsAssignableTo(typeof(Component)))
                    return;

                string label = GetDisplayName(property, elementType);

                var inputListView = new PortListView(label, index =>
                {
                    Port port = _node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, elementType);

                    port.portName = $"Element {index}";

                    return port;
                });

                _node.inputContainer.Add(inputListView);

                property = property.FindSelf();
                 
                Connect += () => _manager.AddListViewInput(inputListView, property, elementGetter);
            }

            void AddInput(SerializedProperty property)
            {
                FieldInfo field = property.GetField();

                if (field == null)
                    return;

                Type propertyType = field.FieldType;

                SerializedProperty initialProperty = property;

                if (propertyType.HasGenericTypeDefinition(typeof(Interface<>)))
                {
                    propertyType = propertyType.GetFirstGenericArgument();
                    property = property.FindPropertyRelative("_underlyingValue");
                }
                else
                {
                    if (field.TryGetCustomAttribute(out InterfaceAttribute interfaceAttribute))
                        propertyType = interfaceAttribute.Type;
                }

                if (!propertyType.IsInterface && !propertyType.IsAssignableTo(typeof(Component)))
                    return;

                Port input = _node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, propertyType);

                input.portName = GetDisplayName(initialProperty, propertyType);

                _node.inputContainer.Add(input);

                property = property.FindSelf();

                Connect += () => _manager.AddInput(input, property);
            }

            foreach (var property in _object.VisibleProperties())
            {
                if (property.isArray)
                    AddListViewInput(property);
                else if (property.propertyType == SerializedPropertyType.ObjectReference || property.propertyType == SerializedPropertyType.Generic)
                    AddInput(property);
            }
        }

        public void AddDropdown(bool collapsed)
        {
            if(_inspectorContainer != null)
            {
                void SetCollapsed(bool isCollapsed)
                {
                    DisplayStyle display;

                    if (isCollapsed)
                        display = DisplayStyle.None;
                    else
                        display = DisplayStyle.Flex;

                    _inspectorContainer.style.display = display;
                }

                var dropdownButton = new Toggle();

                dropdownButton.AddToClassList("unity-foldout__toggle");

                dropdownButton.value = !collapsed;

                _node.titleButtonContainer.Insert(0, dropdownButton);

                SetCollapsed(collapsed);

                dropdownButton.RegisterValueChangedCallback(changeEvent =>
                {
                    _node.Collapsed = !changeEvent.newValue;

                    SetCollapsed(!changeEvent.newValue);
                });
            }
            else
            {
                var dropdownSpace = new VisualElement();

                dropdownSpace.AddToClassList("unity-foldout__toggle");

                _node.titleButtonContainer.Insert(0, dropdownSpace);
            }
        }
        public void SetLocalBound(Rect localBound)
        {
            _node.AddResizableElement(ResizerDirection.Left | ResizerDirection.Right);

            _node.SetPosition(localBound);

            _node.style.width = localBound.width;
        }

    }
}
