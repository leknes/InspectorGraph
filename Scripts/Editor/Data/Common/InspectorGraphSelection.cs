using InspectorGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace InspectorGraph
{
    [InitializeOnLoad]
    public static class InspectorGraphSelection
    {
        private static GameObjectIdentfier _currentId;
        public static GameObject CurrentGameObject { get; private set; }

        public static event Action SelectionChanged;

        static InspectorGraphSelection()
        {
            SetGameObject();

            Selection.selectionChanged += OnSelectionChanged;
        }

        // This method is called when either the selection changes, while the inspector graph is open, or the inspector graph is closed, while a game object is selected.
        // It is meant to update the data representation of the game object, not the id which is handled by the selection alone?
        // However, when this method is called, data should always be saved, which is a little complicated, since data should be copied instead when this method is not called.
        public static void SaveGameObject(GameObjectData data)
        {
            // We don't care about the old id, since, if a component has been added, we want to account for it.
             
            InspectorGraphDatabase.Save(_currentId, data);
        }

        public static bool TryLoadGameObject(out GameObjectData data)
        {
            return InspectorGraphDatabase.TryLoad(_currentId, out data);
        }
        private static void SynchronizeGameObject()
        {
            // When the inspector is opened, there is no need to synchronize the game object, since the graph then is responsible for this via the Update method.

            if (InspectorGraphWindow.IsOpen)
                return;
             
            GameObjectIdentfier currentId = GameObjectIdentfier.From(CurrentGameObject);

            // Since synchronizing just is, saving the data of the game object to the new id, when the id has not changed, this would be redundant.

            if (currentId.Equals(_currentId))
                return;

            // If there is no data there is nothing to synchronize.

            if (!InspectorGraphDatabase.TryLoad(_currentId, out GameObjectData data))
                return;

            // Look, look! No the data is indeed synchronized with the new id.
            // We could delete the entry for the old id now, but I don't really feel like it.

            InspectorGraphDatabase.Save(currentId, data);
        }

        private static void SetGameObject()
        {
            CurrentGameObject = Selection.activeGameObject;

            if (CurrentGameObject != null)
                _currentId = GameObjectIdentfier.From(CurrentGameObject);
        }

        private static void OnSelectionChanged()
        {
            if (CurrentGameObject != null)
                SynchronizeGameObject();

            SetGameObject();

            if (SelectionChanged != null)
                SelectionChanged();
        }
    }

    public struct SelectionChangedEventArgs
    {
        public GameObject PreviousGameObject;
        public GameObject CurrentGameObject;
    }
}
