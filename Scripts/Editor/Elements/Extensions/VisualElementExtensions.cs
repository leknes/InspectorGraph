using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace InspectorGraph
{
    public static class VisualElementExtensions
    {
        public static void RegisterDetachFromPanelCallback(this VisualElement element, Action callback)
        {

            element.RegisterCallback<DetachFromPanelEvent>(detachFromPanelEvent =>
            {
                EditorApplication.delayCall += () =>
                {
                    if (element.panel == null)
                        callback();
                };
            });
        }

        public static void AddResizableElement(this VisualElement element, ResizerDirection direction)
        {
            var resizable = new ResizableElement();

            resizable.SetDirection(direction);

            element.Add(resizable);
        }

        public static void SetDirection(this ResizableElement element, ResizerDirection direction)
        {
            if (!direction.HasFlag(ResizerDirection.Bottom))
            {
                element[0].RemoveAt(2);
                element[1].RemoveAt(2);
                element[2].RemoveAt(2);
            }

            if (!direction.HasFlag(ResizerDirection.Top))
            {
                element[0].RemoveAt(0);
                element[1].RemoveAt(0);
                element[2].RemoveAt(0);
            }

            if (!direction.HasFlag(ResizerDirection.Right))
                element.RemoveAt(2);

            if (!direction.HasFlag(ResizerDirection.Left))
                element.RemoveAt(0);
        }

        /*
        public static void SetLocalBound(this  VisualElement element, Rect localBound)
        {
            element.style.left = localBound.x;
            element.style.top = localBound.y;

            element.style.width = localBound.width;
            element.style.height = localBound.height;
        }
        */
    }
}