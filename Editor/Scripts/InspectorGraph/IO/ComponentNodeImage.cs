using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    [Serializable]
    public struct ComponentNodeImage
    {
        public ComponentNodeImage(bool isCollapsed, Rect localBound)
        {
            IsCollapsed = isCollapsed;
            LocalBound = localBound;
        }

        public bool IsCollapsed;

        public Rect LocalBound;
    }
}