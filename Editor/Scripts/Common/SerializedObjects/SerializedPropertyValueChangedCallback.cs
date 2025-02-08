using Senkel;
using Senkel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public static class SerializedPropertyValueChangedCallback
    {
        // This will only proceed to work, if the `MultiDictionary` collection automatically removes keys that have no values assigned to them. 

        private readonly static MultiDictionary<SerializedObject, ValueChangedTracker, Action> _trackedSerializedObjects = new MultiDictionary<SerializedObject, ValueChangedTracker, Action>(null, ValueChangedTracker.EqualityComparer.Instance);

        private static void CheckSerializedObjects()
        {
            var destroyedSerializedObjects = new List<SerializedObject>(4);

            foreach (var (serializedObject, trackers) in _trackedSerializedObjects)
            {
                if (serializedObject.targetObject == null)
                {
                    destroyedSerializedObjects.Add(serializedObject);

                    continue;
                }

                if (!serializedObject.hasModifiedProperties)
                    serializedObject.Update();

                foreach (var (tracker, callbacks) in trackers)
                {
                    if (!tracker.CheckAndUpdateValue())
                        continue;

                    foreach (var callback in callbacks)
                        callback();
                }
            }

            foreach (var serializedObject in destroyedSerializedObjects)
                _trackedSerializedObjects.Remove(serializedObject);

            if (_trackedSerializedObjects.Count == 0)
                EditorApplication.update -= CheckSerializedObjects;

        }

        public static void RegisterValueChangedCallback(this SerializedProperty property, Action callback)
        {
            if (_trackedSerializedObjects.Count == 0)
                EditorApplication.update += CheckSerializedObjects;

            ValueChangedTracker tracker;

            if (property.isArray)
                tracker = new ArrayValueChangedTracker(property);
            else
                tracker = new BoxedValueChangedTracker(property);

            _trackedSerializedObjects.Add(property.serializedObject, tracker, callback);
        }

        public static void RegisterValueChangedCallback<T>(this SerializedProperty property, VisualElement owner, Action callback) where T : EditorWindow
        {
            property.RegisterValueChangedCallback(callback);

            owner.RegisterDetachFromPanelCallback(() =>
            {
                property.UnregisterValueChangedCallback(callback);
            });
        }

        public static void UnregisterValueChangedCallback(this SerializedProperty property, Action callback)
        {
            if (!_trackedSerializedObjects.Remove(property.serializedObject, new ComparerValueChangedTracker(property)))
                return;

            if (_trackedSerializedObjects.Count == 0)
                EditorApplication.update -= CheckSerializedObjects;
        }

        private class ComparerValueChangedTracker : ValueChangedTracker
        {
            public override SerializedProperty Property { get; }

            public ComparerValueChangedTracker(SerializedProperty property)
            {
                Property = property;
            }

            public override bool CheckAndUpdateValue() => false;
            public override void UpdateValue() { }
        }

        private abstract class ValueChangedTracker
        {
            public abstract SerializedProperty Property { get; }

            public abstract bool CheckAndUpdateValue();
            public abstract void UpdateValue();

            public class EqualityComparer : IEqualityComparer<ValueChangedTracker>
            {
                private EqualityComparer() { }

                public static EqualityComparer Instance { get; } = new EqualityComparer();

                public bool Equals(ValueChangedTracker right, ValueChangedTracker left)
                {
                    return right.Property == left.Property;
                }

                public int GetHashCode(ValueChangedTracker tracker)
                {
                    return tracker.Property.GetHashCode();
                }
            }
        }

        private class BoxedValueChangedTracker : ValueChangedTracker
        {
            private object _previousValue;

            public override SerializedProperty Property { get; }

            public BoxedValueChangedTracker(SerializedProperty property)
            {
                Property = property;

                UpdateValue();
            }

            public override bool CheckAndUpdateValue()
            {
                var value = Property.boxedValue;

                if (NullableContext.Equals(_previousValue, value))
                    return false;

                _previousValue = value;

                return true;
            }
            public override void UpdateValue()
            {
                _previousValue = Property.boxedValue;
            }
        }


        private class ArrayValueChangedTracker : ValueChangedTracker
        {
            private BoxedValueChangedTracker[] _previousArray;

            public override SerializedProperty Property { get; }

            public ArrayValueChangedTracker(SerializedProperty property)
            {
                Property = property;

                _previousArray = CreateArray();
            }

            private BoxedValueChangedTracker[] CreateArray()
            {
                if (Property.arraySize == 0)
                    return new BoxedValueChangedTracker[0];

                var previousArray = new BoxedValueChangedTracker[Property.arraySize];

                for (int i = 0; i < previousArray.Length; i++)
                    previousArray[i] = new BoxedValueChangedTracker(Property.GetArrayElementAtIndex(i));

                return previousArray;
            }

            public override bool CheckAndUpdateValue()
            {
                int previousArrayLength = _previousArray.Length;

                if (previousArrayLength == Property.arraySize)
                {
                    int index = previousArrayLength;

                    for (int i = 0; i < previousArrayLength; i++)
                    {
                        if (!_previousArray[i].CheckAndUpdateValue())
                            continue;

                        index = i;

                        break;
                    }

                    for (int i = index + 1; i < previousArrayLength; i++)
                        _previousArray[i].UpdateValue();

                    return index != previousArrayLength;
                }

                ResizePreviousArray();

                return true;
            }

            public override void UpdateValue()
            {
                int previousArrayLength = _previousArray.Length;

                if (previousArrayLength == Property.arraySize)
                {
                    for (int i = 0; i < previousArrayLength; i++)
                        _previousArray[i].UpdateValue();
                }

                ResizePreviousArray();
            }

            private void ResizePreviousArray()
            {
                int previousArrayLength = _previousArray.Length;

                if (previousArrayLength < Property.arraySize)
                {
                    // When new elements have been added, the existing elements still have to be checked, in order to update them.
                    // Then, new trackers are added for the added elements.

                    for (int i = 0; i < previousArrayLength; i++)
                        _previousArray[i].UpdateValue();

                    Array.Resize(ref _previousArray, Property.arraySize);

                    for (int i = previousArrayLength; i < Property.arraySize; i++)
                        _previousArray[i] = new BoxedValueChangedTracker(Property.GetArrayElementAtIndex(i));
                }
                else
                {
                    // When elements have been removed, only the trackers have to be updated, that still exists.
                    // All other elements which do not exist anymore are removed.

                    for (int i = 0; i < Property.arraySize; i++)
                        _previousArray[i].UpdateValue();

                    Array.Resize(ref _previousArray, Property.arraySize);
                }
            }
        }
    }
}