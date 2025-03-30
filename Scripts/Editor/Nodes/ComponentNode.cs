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
    public class ComponentNode : Node
    {
        public const float InitialWidth = 448;

        private readonly GraphView _graphView;

        public ComponentNode(GraphView graphView, Component component)
        {
            _graphView = graphView;

            Component = component;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (Component is not Transform)
            {
                evt.menu.AppendAction("Remove Component", _ =>
                {
                    _graphView.RemoveElement(this);

                    Undo.DestroyObjectImmediate(Component);
                });
            }
        }
         
        public bool Collapsed { get; set; }
        public Component Component { get; }
    }
}
