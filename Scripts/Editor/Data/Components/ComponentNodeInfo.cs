using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InspectorGraph
{ 
    [Serializable]
    public struct ComponentNodeInfo
    {
        public ComponentNodeData Data;

        public Component Component;
    }
}


