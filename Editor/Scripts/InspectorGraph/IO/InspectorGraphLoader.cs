using Senkel.Collections;
using Senkel.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public static class InspectorGraphLoader
    {
        private static readonly Dictionary<GameObjectID, InspectorGraphImage> _cache = new Dictionary<GameObjectID, InspectorGraphImage>();

        private readonly static string _directoryPath = $"{Application.persistentDataPath}/InspectorGraph/Editor/Data";

        private static string GetPath(GameObjectID id)
        {
            return $"{_directoryPath}/{id}.json";
        }

        public static void Save(GameObjectID id, InspectorGraphImage image)
        {
            _cache.Ensure(id, image);

            if (!Directory.Exists(_directoryPath))
                Directory.CreateDirectory(_directoryPath);

            File.WriteAllText(GetPath(id), JsonUtility.ToJson(image));
        }

        public static void Delete(GameObjectID id)
        {
            _cache.Remove(id);

            string path = GetPath(id);

            if (!File.Exists(path))
                return;

            File.Delete(path);
        }

        public static bool TryLoad(GameObjectID id, out ComponentNodeLoader loader)
        {
            if (_cache.TryMatch(id, out InspectorGraphImage image))
            {
                loader = new ComponentNodeLoader(image);

                return true;
            }

            string path = GetPath(id);

            if (!File.Exists(path))
            {
                loader = new ComponentNodeLoader(new InspectorGraphImage(new ComponentImage[0]));

                return false;
            }

            loader = new ComponentNodeLoader(JsonUtility.FromJson<InspectorGraphImage>(File.ReadAllText(path)));

            return true;
        }
    }

    public class ComponentNodeLoader
    {
        private readonly Dictionary<string, List<ComponentNodeImage>> _components;

        public ComponentNodeLoader(InspectorGraphImage image)
        {
            _components = new Dictionary<string, List<ComponentNodeImage>>(image.Components.Length);

            foreach (var component in image.Components)
                _components.Add(component.FullName, new List<ComponentNodeImage>(component.Nodes ?? new ComponentNodeImage[0]));
        }

        public bool TryPopNode(string fullName, out ComponentNodeImage nodeImage)
        {
            if (_components.TryGetValue(fullName, out var nodeImages) && nodeImages.Count > 0)
            {
                nodeImage = nodeImages[0];
                nodeImages.RemoveAt(0);

                return true;
            }

            nodeImage = default;

            return false;
        }
    }
}