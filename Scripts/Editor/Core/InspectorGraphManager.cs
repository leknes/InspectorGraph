using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;

namespace InspectorGraph
{
    public class InspectorGraphManager
    {
        public GraphView GraphView { get; }

        private readonly Dictionary<Port, SerializedProperty> _inputs = new Dictionary<Port, SerializedProperty>();

        private readonly Dictionary<Component, Port> _outputs = new Dictionary<Component, Port>();
        private readonly Dictionary<Port, Component> _components = new Dictionary<Port, Component>();

        private readonly static Color _defaultPortColor = new Color(0.7686275f, 0.7686275f, 0.7686275f);

        public InspectorGraphManager(GraphView graphView)
        {
            GraphView = graphView;

            graphView.graphViewChanged += graphViewChange =>
            {
                if (graphViewChange.elementsToRemove != null)
                    ManipulateElementsToRemove(graphViewChange.elementsToRemove);

                if (graphViewChange.edgesToCreate != null)
                    ManipulateEdgesToCreate(graphViewChange.edgesToCreate);

                return graphViewChange;
            };
        }

        private void ManipulateElementsToRemove(List<GraphElement> elementsToRemove)
        {
            foreach (GraphElement element in elementsToRemove)
            {
                if (element is not Edge edge)
                    continue;

                SerializedProperty property = _inputs[edge.input];
                property.objectReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            }


        }

        private void ManipulateEdgesToCreate(List<Edge> edgesToCreate)
        {
            foreach (Edge edge in edgesToCreate)
            {
                edge.input.portColor = _defaultPortColor;

                SerializedProperty property = _inputs[edge.input];
                property.objectReferenceValue = _components[edge.output];
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public void AddListViewInput(PortListView inputListView, SerializedProperty property, Func<SerializedProperty, SerializedProperty> converter)
        {
            // Let's just hope this does actually work.

            int previousArraySize = property.arraySize;

            Action elementValueChanged = null;

            void RegisterElementInput(Port input, SerializedProperty property)
            {
                Action callback = AddInputNoCallback(input, property);

                elementValueChanged += callback;

                input.RegisterDetachFromPanelCallback(() => elementValueChanged -= callback);
            }

            void AddElementInput(int index)
            {
                RegisterElementInput(inputListView.AddPort(), converter(property.GetArrayElementAtIndex(index)));
            }

            void OnArraySizeChanged()
            {
                // This is done, so when the port has already been added/removed, no new port is added/removed here.

                if (inputListView.PortCount == property.arraySize)
                    return;

                if (property.arraySize < previousArraySize)
                {
                    for (int i = property.arraySize; i < previousArraySize; i++)
                        DisconnectAll(inputListView.RemovePort());
                }
                else
                {
                    for (int i = previousArraySize; i < property.arraySize; i++)
                        AddElementInput(i);
                }
            }

            void OnValueChanged()
            {
                if (previousArraySize != property.arraySize)
                {
                    OnArraySizeChanged();

                    previousArraySize = property.arraySize;
                }
                else if (elementValueChanged != null)
                    elementValueChanged();
            }

            inputListView.Added += input =>
            {
                var elementProperty = converter(property.AddArrayElement());

                property.serializedObject.ApplyModifiedProperties();

                RegisterElementInput(input, elementProperty);
            };

            inputListView.Removed += input =>
            {
                DisconnectAll(input);

                property.DeleteArrayElementAtIndex(property.arraySize - 1);

                property.serializedObject.ApplyModifiedProperties();
            };

            property.RegisterValueChangedCallback<InspectorGraphWindow>(inputListView, OnValueChanged);

            for (int i = 0; i < property.arraySize; i++)
                AddElementInput(i);
        }

        private void DisconnectAll(Port port)
        {
            var connections = port.connections.ToArray();

            foreach (var edge in connections)
            {
                edge.output.Disconnect(edge);
                edge.input.Disconnect(edge);

                GraphView.RemoveElement(edge);
            }
        }

        private Action AddInputNoCallback(Port input, SerializedProperty property)
        {
            input.portColor = _defaultPortColor;

            void ConnectTo(Port port)
            {
                Edge edge = input.ConnectTo(port);

                GraphView.AddElement(edge);
            }

            void ApplyModifiedProperty()
            {
                var objectReferenceValue = property.objectReferenceValue as Component;

                if (objectReferenceValue == null)
                    return;

                if (!_outputs.TryGetValue(objectReferenceValue, out Port output))
                    return;

                ConnectTo(output);
            }

            void OnValueChanged()
            {
                DisconnectAll(input);

                ApplyModifiedProperty();
            }

            ApplyModifiedProperty();

            _inputs.Add(input, property);

            input.RegisterDetachFromPanelCallback(() =>
            {
                _inputs.Remove(input);

                DisconnectAll(input);
            });

            return OnValueChanged;
        }

        public void AddInput(Port input, SerializedProperty property)
        {
            property.RegisterValueChangedCallback<InspectorGraphWindow>(input, AddInputNoCallback(input, property));
        }


        // This method is not required, but I feel safer having it.
        public void ClearAll()
        {
            _inputs.Clear();
            _outputs.Clear();
            _components.Clear();
        }

        public void AddOutput(Port output, Component component)
        {
            output.portColor = _defaultPortColor;

            Debug.Log($"Adding output... ({_components.Count})");

            _outputs.Add(component, output);
            _components.Add(output, component);

            output.RegisterDetachFromPanelCallback(() =>
            {
                _outputs.Remove(component);
                _components.Remove(output);

                DisconnectAll(output);
            });
        }

    }
}
