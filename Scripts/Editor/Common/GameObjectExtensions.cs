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
    }
}
