using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InspectorGraph
{
    public struct GameObjectIdentfier : IEquatable<GameObjectIdentfier>
    {
        public static GameObjectIdentfier From(GameObject gameObject)
        {
            return new GameObjectIdentfier
            { 
                Component = ComponentIdentifier.From(gameObject.GetComponentsNoNull()),
                Label = LabelIdentifier.From(gameObject)
            };
        }

        public bool Equals(GameObjectIdentfier identifier)
        {
            return Component.Equals(identifier.Component) && Label.Equals(identifier.Label);
        }

        public ComponentIdentifier Component;

        public LabelIdentifier Label;

        public struct ComponentIdentifier
        {
            public static ComponentIdentifier From(IReadOnlyList<Component> componentCollection)
            {
                string[] nameArray = new string[componentCollection.Count];

                for (int i = 0; i < componentCollection.Count; i++) 
                    nameArray[i] = componentCollection[i].GetType().Name;
            
                Array.Sort(nameArray, StringComparer.OrdinalIgnoreCase);

                Span<char> order = stackalloc char[componentCollection.Count];
                int hashcode = 0;

                for (int i = 0; i < componentCollection.Count; i++)
                {
                    string name = nameArray[i];

                    order[i] = name[0];
                    hashcode ^= name.GetHashCode();
                }

                return new ComponentIdentifier(new string(order), hashcode);
            }
             
            public ComponentIdentifier(string order, int hashCode)
            {
                Order = order;
                HashCode = hashCode;
            }

            public readonly string Order;

            public readonly int HashCode;

            public override bool Equals(object obj)
            {
                return obj is ComponentIdentifier identifier && Equals(identifier);
            }

            public bool Equals(ComponentIdentifier identifier)
            {
                return Order == identifier.Order && HashCode == identifier.HashCode;
            }

            public override int GetHashCode()
            {
                return HashCode;
            }

            public override string ToString()
            {
                return $"{Order}_{HashCode}";
            }
        }

        public struct LabelIdentifier
        {
            public static LabelIdentifier From(GameObject gameObject)
            {
                return new LabelIdentifier(gameObject.scene, gameObject.name);
            }

            public bool Equals(LabelIdentifier identifier)
            {
                return Scene == identifier.Scene && Name == identifier.Name;
            }

            public LabelIdentifier(Scene scene, string name)
            {
                Scene = scene;
                Name = name;
            }

            public Scene Scene;

            public string Name;
             
            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public override string ToString()
            {
                return $"{Scene.buildIndex}_{Name}";
            }
        }
         
    }
}
