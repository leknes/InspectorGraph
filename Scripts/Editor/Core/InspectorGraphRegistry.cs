using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectorGraph
{
    public class InspectorGraphRegistry
    {
        private readonly Stack<ComponentNodeData> _unregisteredNodes = new Stack<ComponentNodeData>();

        private readonly List<ComponentNode> _nodes = new List<ComponentNode>();
        public IReadOnlyList<ComponentNode> Nodes => _nodes;

        public void Clear()
        {
            _unregisteredNodes.Clear();
            _nodes.Clear();
        }

        public void RegisterNode(ComponentNode node)
        {
            _nodes.Add(node);
        }

        public void UnregisterNode(ComponentNode node)
        {
            _unregisteredNodes.Push(node.ToData());
            _nodes.Remove(node);
        }

        public bool TryRecoverNode(out ComponentNodeData data)
        { 
            return _unregisteredNodes.TryPop(out data);
        }
    }
}
