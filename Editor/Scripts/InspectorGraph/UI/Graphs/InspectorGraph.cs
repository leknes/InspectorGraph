using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System.Drawing;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public class InspectorGraph : EditorWindow
    {
        private InspectorGraphView _graphView;

        [MenuItem("Window/General/Inspector Graph")]
        public static void OpenInspectorGraphWindow()
        {
            var window = GetWindow<InspectorGraph>();

            var icon = Resources.Load<Texture>("Textures/InspectorGraphIcon");

            window.titleContent = new GUIContent("Inspector Graph", icon);
        }

        private void AddGraphView()
        {
            // Here the ID of the game object should be captured, and the corresponding graph view data be loaded.
            // When the graph view is closed again, all this stuff should be saved instead.
            _graphView = new InspectorGraphView()
            {
                name = "Inspector Graph"
            };

            _graphView.StretchToParentSize();

            rootVisualElement.Add(_graphView);
        }

        private void LoadGraphView()
        {
            GameObject activeGameObject = Selection.activeGameObject;

            if (activeGameObject == null)
                return;

            _graphView.LoadGameObject(activeGameObject);
        }

        private void ReloadGraphView()
        {
            _graphView.Unload();

            LoadGraphView();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += ReloadGraphView;

            AddGraphView();

            LoadGraphView();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= ReloadGraphView;

            rootVisualElement.Remove(_graphView);
        }

        /*

        private void InitializeToolbar()
        {
            // This toolbar will probably need a "Reset" and "Save" method.

            var toolbar = new Toolbar();

            toolbar.Add(InitializeSaveButton());
            toolbar.Add(InitializeRevertButton());

            rootVisualElement.Add(toolbar);
        }

        private ToolbarButton InitializeSaveButton()
        {
            var saveButton = new ToolbarButton(() => { });

            var icon = Resources.Load<Texture2D>("Textures/SaveIcon");

            saveButton.iconImage = Background.FromTexture2D(icon);
            saveButton.text = "Save";
            saveButton.style.paddingLeft = 3; 

            return saveButton;
        }

        private ToolbarButton InitializeRevertButton()
        {
            var resetButton = new ToolbarButton(() => { });

            var icon = Resources.Load<Texture2D>("Textures/RevertIcon");

            resetButton.iconImage = Background.FromTexture2D(icon);
            resetButton.text = "Revert";

            return resetButton;
        }

        */
    }
}