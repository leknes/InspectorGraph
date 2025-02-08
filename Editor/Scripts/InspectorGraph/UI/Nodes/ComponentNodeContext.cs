using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public class ComponentNodeContext
    {
        public ComponentPortManager PortManager { get; }

        public event Action ComponentsInitialized;

        public ComponentNodeContext(ComponentPortManager portManager)
        {
            PortManager = portManager;
        }

        public void OnComponentsInitialized()
        {
            if (ComponentsInitialized != null)
                ComponentsInitialized();
        }
    }
}