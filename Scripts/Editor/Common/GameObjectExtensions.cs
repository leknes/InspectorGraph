using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InspectorGraph
{
    public static class GameObjectExtensions
    {
        public static Component[] GetComponents(this GameObject gameObject)
        {
            return gameObject.GetComponents<Component>();
        }

        public static List<Component> GetComponentsNoNull(this GameObject gameObject)
        {
            var componentArray = gameObject.GetComponents();

            List<Component> componentList = new List<Component>(componentArray.Length);

            for (int i = 0; i < componentArray.Length; i++)
            {
                if (componentArray[i] == null)
                    continue;

                componentList.Add(componentArray[i]);
            }

            return componentList;
        }
    }
}
