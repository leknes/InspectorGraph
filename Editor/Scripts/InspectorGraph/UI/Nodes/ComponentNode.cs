using Senkel;
using Senkel.Reflection;
using Senkel.Unity.Extensions.InspectorGraph.Editor;
using Senkel.Unity.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.Object;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public partial class ComponentNode : Node
    {
        private static readonly StyleSheet _nodeStyle = Resources.Load<StyleSheet>("Styles/Node");
        private static readonly StyleSheet _inspectorElementStyle = Resources.Load<StyleSheet>("Styles/InspectorElement");

        public const float InitialWidth = 448;

        private const float _initialMinWidth = 384;

        private InspectorGraphView _graphView;

        public bool IsCollapsed { get; private set; }
        public Type Type { get; private set; }

        public Component Component { get; private set; }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (Component is not Transform)
            {
                evt.menu.AppendAction("Remove Component", _ =>
                {
                    _graphView.RemoveElement(this);

                    Undo.DestroyObjectImmediate(Component);
                });
            }
        }

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            Port port = base.InstantiatePort(orientation, direction, capacity, type);

            return port;
        }

        public static ComponentNode Create(InspectorGraphView graphView, ComponentNodeContext context, Component component, Type type, Rect localBound, bool isCollapsed)
        {
            var serializedComponent = new SerializedObject(component);

            var node = new ComponentNode();

            node._graphView = graphView;

            node.Component = component;

            node.Type = type;
            node.IsCollapsed = isCollapsed;

            node.styleSheets.Add(_nodeStyle);

            node.title = DisplayName.Type(type);

            node.style.minWidth = _initialMinWidth;

            var inspectorElement = new InspectorElement(serializedComponent);

            var newTitleContainer = new VisualElement() { name = "new-title" };

            newTitleContainer.style.flexDirection = FlexDirection.Row;

            SetTitleContainer();

            void SetTitleContainer()
            {
                node.mainContainer.Insert(0, newTitleContainer);
                node.mainContainer.Remove(node.titleContainer);

                newTitleContainer.Add(node.titleContainer);

                node.titleContainer.Insert(0, node.titleButtonContainer);
                node.titleButtonContainer.RemoveAt(0);
            }

            void FixListView()
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

                node.RegisterCallback<PointerDownEvent>(pointerDownEvent =>
                {
                    if (IsListView((VisualElement)pointerDownEvent.target))
                        pointerDownEvent.StopPropagation();
                });
            }

            void AddIcon()
            {
                Texture icon = null;

                UnityLogger.InvokeWithLogDisabled(() => icon = EditorGUIUtility.IconContent($"{type.Name} Icon").image);

                if (icon == null)
                {
                    if (component is not MonoBehaviour)
                        return;

                    if (type.Namespace != null && (type.Namespace.StartsWith("UnityEngine") || type.Namespace.StartsWith("UnityEditor")))
                        return;

                    icon = EditorGUIUtility.IconContent($"cs Script Icon").image;
                }

                node.titleContainer.Insert(1, new Image() { image = icon });
            }

            void AddEnabled()
            {
                SerializedProperty enabled = serializedComponent.FindProperty("m_Enabled");

                if (enabled == null)
                    return;

                var toggle = new Toggle();

                toggle.BindProperty(enabled);

                node.titleContainer.Insert(node.titleContainer.childCount - 1, toggle);
            }


            void SetComponent()
            {
                var inspectorContainer = new VisualElement() { name = "inspector" };

                node.Add(inspectorContainer);

                inspectorContainer.Add(inspectorElement);

                if (!inspectorElement.HasIMGUIContainer())
                    FixListView();

                node.styleSheets.Add(_inspectorElementStyle);

                var imguiContainer = inspectorElement.GetIMGUIContainer();

                void FixIMGUIContainer()
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

                if (imguiContainer != null)
                    FixIMGUIContainer();

                void AddDropDown()
                {
                    void SetCollapsed(bool isCollapsed)
                    {
                        DisplayStyle display;

                        if (isCollapsed)
                            display = DisplayStyle.None;
                        else
                            display = DisplayStyle.Flex;

                        inspectorContainer.style.display = display;
                    }

                    var dropdownButton = new Toggle();

                    dropdownButton.AddToClassList("unity-foldout__toggle");

                    dropdownButton.value = !isCollapsed;

                    node.titleButtonContainer.Insert(0, dropdownButton);

                    SetCollapsed(isCollapsed);

                    dropdownButton.RegisterValueChangedCallback(changeEvent =>
                    {
                        node.IsCollapsed = !changeEvent.newValue;

                        SetCollapsed(!changeEvent.newValue);
                    });
                }

                AddDropDown();

                SetInput();
            }

            void SetOutput()
            {
                Port output = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);

                output.portName = string.Empty;

                var newOutputContainer = new VisualElement() { name = "new-output" };

                newTitleContainer.Add(newOutputContainer);
                newOutputContainer.Add(output);

                context.PortManager.AddOutput(output, component);
            }

            void SetInput()
            {
                string GetDisplayName(SerializedProperty property, Type propertyType)
                {
                    return $"{property.displayName} ({DisplayName.Type(propertyType)})";
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
                        elementType = propertyType.GetFirstGenericArgument();

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
                        Port port = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, elementType);

                        port.portName = $"Element {index}";

                        return port;
                    });

                    node.inputContainer.Add(inputListView);

                    property = property.ToPersistent();

                    context.ComponentsInitialized += () => context.PortManager.AddListViewInput(inputListView, property, elementGetter);
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

                    Port input = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, propertyType);

                    input.portName = GetDisplayName(initialProperty, propertyType);

                    node.inputContainer.Add(input);

                    property = property.ToPersistent();

                    context.ComponentsInitialized += () => context.PortManager.AddInput(input, property);
                }

                foreach (var property in serializedComponent.VisibleProperties())
                {
                    if (property.isArray)
                        AddListViewInput(property);
                    else if (property.propertyType == SerializedPropertyType.ObjectReference || property.propertyType == SerializedPropertyType.Generic)
                        AddInput(property);
                }
            }

            AddEnabled();
            AddIcon();

            SetOutput();

            if (!inspectorElement.IsEmpty())
                SetComponent();
            else
            {
                var dropdownSpace = new VisualElement();

                dropdownSpace.AddToClassList("unity-foldout__toggle");

                node.titleButtonContainer.Insert(0, dropdownSpace);
            }

            node.AddResizableElement(ResizerDirection.Left | ResizerDirection.Right);

            node.SetPosition(localBound);

            node.style.width = localBound.width;

            return node;
        }

    }
}