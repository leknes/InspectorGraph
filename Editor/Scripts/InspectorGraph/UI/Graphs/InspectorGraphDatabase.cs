using Senkel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Senkel.Unity.Extensions.InspectorGraph.Editor
{
    public class InspectorGraphDatabase
    {
        private readonly Stack<ComponentNodeImage> _removedNodes = new Stack<ComponentNodeImage>();

        private readonly List<ComponentNode> _nodes = new List<ComponentNode>();

        public IReadOnlyList<ComponentNode> Nodes => _nodes;

        public InspectorGraphImage ToImage()
        {
            var dictionary = new MultiDictionary<string, ComponentNodeImage>();

            foreach (var node in _nodes)
            {
                string fullName = node.Type.FullName;

                dictionary.Add(fullName, new ComponentNodeImage(node.IsCollapsed, node.localBound));
            }

            var components = new ComponentImage[dictionary.Count];

            int index = 0;

            foreach (var (fullName, nodeImages) in dictionary)
            {
                components[index] = new ComponentImage(fullName, nodeImages.ToArray());

                index++;
            }

            return new InspectorGraphImage(components);
        }

        public void Clear()
        {
            _removedNodes.Clear();
            _nodes.Clear();
        }

        public void AddNode(ComponentNode node)
        {
            _nodes.Add(node);
        }

        public void RemoveNode(ComponentNode node)
        {
            _removedNodes.Push(new(node.IsCollapsed, node.localBound));

            _nodes.Remove(node);
        }

        public bool TryRecoverNode(out ComponentNodeImage image)
        {
            return _removedNodes.TryPop(out image);
        }

    }
}