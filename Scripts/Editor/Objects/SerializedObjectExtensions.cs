using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace InspectorGraph
{
    public static class SerializedObjectExtensions
    {
        public static IEnumerable<SerializedProperty> VisibleProperties(this SerializedObject serializedObject, bool enterChildren = false)
        {
            var property = serializedObject.GetIterator();

            if (!property.NextVisible(true))
                yield break;

            do
            {
                yield return property;
            }
            while (property.NextVisible(enterChildren));
        }

    }
}