using Senkel.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InspectorGraph
{
    public static class InspectorGraphDatabase
    {
        private static readonly string _directoryPath = $"{Application.dataPath}/../ProjectSettings/InspectorGraph";

        private static readonly string _primaryDirectoryPath = $"{_directoryPath}/Primary";
        private static readonly string _secondaryDirectoryPath = $"{_directoryPath}/Secondary";

        private static Dictionary<GameObjectIdentfier.ComponentIdentifier, GameObjectData> _primaryCache = new Dictionary<GameObjectIdentfier.ComponentIdentifier, GameObjectData>(4);
        private static Dictionary<GameObjectIdentfier.LabelIdentifier, GameObjectData> _secondaryCache = new Dictionary<GameObjectIdentfier.LabelIdentifier, GameObjectData>(4);

        private static string GetPath(GameObjectIdentfier.ComponentIdentifier identifier) => $"{_primaryDirectoryPath}/{identifier}.json";
        private static string GetPathFallback(GameObjectIdentfier.LabelIdentifier identifier) => $"{_secondaryDirectoryPath}/{identifier}.json";

        public static bool TryLoad(GameObjectIdentfier identifier, out GameObjectData data)
        { 
            if (_primaryCache.TryGetValue(identifier.Component, out data))
                return true;

            string path = GetPath(identifier.Component);
             
            bool exists = File.Exists(path);

            if (exists)
                data = JsonUtility.FromJson<GameObjectData>(File.ReadAllText(path));
            else
            { 
                if(_secondaryCache.TryGetValue(identifier.Label, out data))
                    return true;

                path = GetPathFallback(identifier.Label);

                exists = File.Exists(path);

                if (exists)
                    data = JsonUtility.FromJson<GameObjectData>(File.ReadAllText(path));
            }

            return exists;
        }

        public static void Save(GameObjectIdentfier identifier, GameObjectData data)
        {  
            _primaryCache.Ensure(identifier.Component, data);

            if (!Directory.Exists(_primaryDirectoryPath))
                Directory.CreateDirectory(_primaryDirectoryPath);

            string json = JsonUtility.ToJson(data);

            File.WriteAllText(GetPath(identifier.Component), json);
             
            _secondaryCache.Ensure(identifier.Label, data);

            if (!Directory.Exists(_secondaryDirectoryPath))
                Directory.CreateDirectory(_secondaryDirectoryPath);

            File.WriteAllText(GetPathFallback(identifier.Label), json);
        }



    }
}