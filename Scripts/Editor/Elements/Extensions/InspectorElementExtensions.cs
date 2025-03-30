using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InspectorGraph
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

            if (container is not IMGUIContainer imguiContainer)
                return container.childCount == 0;

            return false;
        }
    }
}