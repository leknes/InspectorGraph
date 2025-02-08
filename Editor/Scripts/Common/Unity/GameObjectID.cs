using Senkel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static System.Math;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public readonly struct GameObjectID : IEquatable<GameObjectID>, IMatchable<GameObjectID>
    {
        public static GameObjectID From(GameObject gameObject)
        {
            Transform transform = gameObject.transform;

            return new GameObjectID(gameObject.name, gameObject.tag, gameObject.layer, gameObject.GetComponentCount(), transform.position, transform.rotation, transform.localScale);
        }

        public GameObjectID(string name, string tag, int layer, int componentCount, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Name = name;
            Tag = tag;
            Layer = layer;
            ComponentCount = componentCount;

            Position = position;
            Scale = scale;
        }

        public string Name { get; }
        public string Tag { get; }
        public int Layer { get; }
        public int ComponentCount { get; }

        public Vector3 Position { get; }
        public Vector3 Scale { get; }

        public override bool Equals(object obj)
        {
            return obj is GameObjectID id && Equals(id);
        }

        public bool Equals(GameObjectID id)
        {
            return Name == id.Name && Tag == id.Tag && Layer == id.Layer && Position == id.Position && Scale == id.Scale;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Tag, Layer, Position, Scale);
        }

        public override string ToString()
        {
            return $"{Name}-{Tag}-{Layer}-{ComponentCount}-{Position}-{Scale}";
        }

        public float Match(GameObjectID id)
        {
            float score = -2.5F;

            if (Name == id.Name)
                score += 3;

            if (ComponentCount == id.ComponentCount)
                score += 2;

            if (Position == id.Position)
                score += 1;

            return score;
        }
    }
}