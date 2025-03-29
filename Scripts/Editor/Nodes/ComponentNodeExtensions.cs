using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectorGraph
{
    public static class ComponentNodeExtensions
    {
        public static ComponentNodeMetadata ToMetadata(this ComponentNode node)
        {
            return new ComponentNodeMetadata()
            {
                Name = node.Component.GetType().Name,
                Data = ToData(node)
            };
        }

        public static ComponentNodeData ToData(this ComponentNode node)
        {
            return new ComponentNodeData
            {
                Collapsed = node.Collapsed,
                LocalBound = node.localBound
            };
        }
    }


}
