using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InspectorGraph
{
    // When instantiating a node there are multiple things to consider.
    // Take note that instantiation is a layered process, consisting of multiple distinctive steps.
    // First there is the instantiation of the actual node independent of any other node.
    // Secondly, there is the wiring up of different nodes, specifically the ports of different nodes.
    // Yeah, that is it. I am not aware of more steps, as of now. Good luck, programming that.

    public class ComponentNodeFactory
    {
        private readonly InspectorGraphManager _manager;

        public ComponentNodeFactory(InspectorGraphManager manager)
        {
            _manager = manager;
        }

        public ComponentNode Create(ComponentNodeInfo info)
        {
            ComponentNodeBuilder builder = new ComponentNodeBuilder(_manager, info);

            builder.Create();

            return builder.Connect();
        }

        public ComponentNode[] Create(IReadOnlyList<ComponentNodeInfo> infoCollection)
        { 
            ComponentNodeBuilder[] builderArray = new ComponentNodeBuilder[infoCollection.Count];

            for (int i = 0; i < infoCollection.Count; i++)
            {
                var builder = new ComponentNodeBuilder(_manager, infoCollection[i]);

                builder.Create();

                builderArray[i] = builder;
            }

            ComponentNode[] nodeArray = new ComponentNode[builderArray.Length];

            for(int i = 0; i < builderArray.Length; i++)
                nodeArray[i] = builderArray[i].Connect();

            return nodeArray;
        }

    }
}


