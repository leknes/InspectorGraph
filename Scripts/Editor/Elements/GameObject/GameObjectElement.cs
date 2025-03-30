using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace InspectorGraph
{
    public class GameObjectElement : GraphElement
    {
        private static readonly StyleSheet _gameObjectElementStyle = Resources.Load<StyleSheet>("Styles/GameObjectElement");

        public GameObjectElement(SerializedObject gameObject)
        {
            capabilities |= Capabilities.Movable;

            AddToClassList("game-object-element");
            styleSheets.Add(_gameObjectElementStyle);
            style.position = Position.Absolute;

            var headerElement = new VisualElement();
            headerElement.AddToClassList("header-element");
            Add(headerElement);

            var gameObjectImage = new Image();
            headerElement.Add(gameObjectImage);

            var isActiveToggle = new Toggle();
            SerializedProperty isActive = gameObject.FindProperty("m_IsActive");
            isActiveToggle.BindProperty(isActive);
            headerElement.Add(isActiveToggle);

            void UpdateGameObjectImage(bool isActive) => gameObjectImage.image = EditorGUIUtility.IconContent(isActive ? $"GameObject On Icon" : $"GameObject Icon").image;

            UpdateGameObjectImage(isActive.boolValue);

            isActiveToggle.RegisterCallback<ChangeEvent<bool>>(changeEvent => UpdateGameObjectImage(changeEvent.newValue));

            var nameField = new TextField();
            nameField.BindProperty(gameObject.FindProperty("m_Name"));
            headerElement.Add(nameField);

            var tagField = new TagField("Tag");
            tagField.BindProperty(gameObject.FindProperty("m_TagString"));
            Add(tagField);

            var layerField = new LayerField("Layer");
            layerField.BindProperty(gameObject.FindProperty("m_Layer"));
            Add(layerField);
        }

    }
}