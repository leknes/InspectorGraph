using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace InspectorGraph
{
    public static class StyleExtensions
    {
        public static void SetBorderColor(this IStyle style, StyleColor color)
        {
            style.borderTopColor = color;
            style.borderBottomColor = color;
            style.borderRightColor = color;
            style.borderLeftColor = color;
        }

        public static void SetMargin(this IStyle style, StyleLength margin)
        {
            style.marginBottom = margin;
            style.marginLeft = margin;
            style.marginRight = margin;
            style.marginTop = margin;
        }

        public static void SetBorderWidth(this IStyle style, StyleFloat borderWidth)
        {
            style.borderBottomWidth = borderWidth;
            style.borderRightWidth = borderWidth;
            style.borderLeftWidth = borderWidth;
            style.borderTopWidth = borderWidth;
        }

        public static void SetBorderRadius(this IStyle style, StyleLength borderRadius)
        {
            style.borderBottomLeftRadius = borderRadius;
            style.borderBottomRightRadius = borderRadius;
            style.borderTopRightRadius = borderRadius;
            style.borderTopLeftRadius = borderRadius;
        }
    }
}