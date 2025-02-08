using Senkel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    [Serializable]
    public class InspectorGraphImage
    {
        public ComponentImage[] Components;

        public InspectorGraphImage(ComponentImage[] components)
        {
            Components = components;
        }

    }


    [Serializable]
    public struct ComponentImage
    {
        public string FullName;
        public ComponentNodeImage[] Nodes;

        public ComponentImage(string fullName, ComponentNodeImage[] nodes)
        {
            FullName = fullName;
            Nodes = nodes;
        }
    }
}