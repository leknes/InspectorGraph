using Senkel.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public static class SerializedPropertyExtensions
    {
        public static SerializedProperty AddArrayElement(this SerializedProperty property)
        {
            return property.InsertAndGetArrayElementAtIndex(property.arraySize);
        }
        public static SerializedProperty InsertAndGetArrayElementAtIndex(this SerializedProperty property, int index)
        {
            property.InsertArrayElementAtIndex(index);

            return property.GetArrayElementAtIndex(index);
        }

        public static SerializedProperty ToPersistent(this SerializedProperty property)
        {
            return property.serializedObject.FindProperty(property.propertyPath);
        }
        public static Type GetPropertyType(this SerializedProperty property)
        {
            // Yeah, that is just a little bit cursed.

            return property.GetField()?.FieldType;
        }

        public static FieldInfo GetField(this SerializedProperty property)
        {
            // Yeah, that is just a little bit cursed.

            return property.serializedObject.targetObject.GetType().GetFieldIncludingBase(property.propertyPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}