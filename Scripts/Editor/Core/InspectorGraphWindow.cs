using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InspectorGraph
{ 
    public class InspectorGraphWindow : EditorWindow
    {
        public static bool IsOpen { get; private set; }

        private InspectorGraphView _graphView;

        public bool Focused => focusedWindow == this;


        [MenuItem("Window/General/Inspector Graph")]
        public static void OpenInspectorGraphWindow()
        { 
            var window = GetWindow<InspectorGraphWindow>();

            var icon = Resources.Load<Texture>("Textures/InspectorGraphIcon");

            window.titleContent = new GUIContent("Inspector Graph", icon);
        }

        private void OnEnable()
        {   
            Debug.Log("Window enabled...");

            IsOpen = true;

            _graphView = new InspectorGraphView(this)
            {
                name = "Inspector Graph"
            };

            _graphView.StretchToParentSize();

            rootVisualElement.Add(_graphView);

            _graphView.Load();
        }

        private void OnDisable()
        {
            _graphView.Unload();

            rootVisualElement.Remove(_graphView);

            IsOpen = false;
        }
    }

}