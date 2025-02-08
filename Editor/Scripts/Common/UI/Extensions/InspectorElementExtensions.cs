using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public static class InspectorElementExtensions
    {
        public static bool HasIMGUIContainer(this InspectorElement element)
        {
            return element[2] is IMGUIContainer;
        }

        public static IMGUIContainer GetIMGUIContainer(this InspectorElement element)
        {
            return element[2] as IMGUIContainer;
        }

        public static bool IsEmpty(this InspectorElement element)
        {
            var container = element[2];

            return container.childCount == 0 && container is not IMGUIContainer;
        }
    }
}