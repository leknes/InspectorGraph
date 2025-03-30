using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace InspectorGraph
{

    public class ComponentNodeBuilder
    {
        private readonly ComponentNodeData _data;

        private readonly ComponentNode _node;

        private readonly ComponentNodeHelper _helper;

        public ComponentNodeBuilder(InspectorGraphManager manager, ComponentNodeInfo info)
        {
            _data = info.Data;

            _node = new ComponentNode(manager.GraphView, info.Component);

            _helper = new ComponentNodeHelper(manager, _node);
        }

        public void Create()
        {
            _helper.SetInitialState();
            _helper.AddTitleContainer();
            _helper.AddToggle();
            _helper.AddIcon();
            _helper.AddOutput();
            _helper.AddInspectorElement();
            _helper.AddInput();
            _helper.AddDropdown(_data.Collapsed);
            _helper.SetLocalBound(_data.LocalBound);
        }
         
        public ComponentNode Connect()
        {
            if(_helper.Connect != null)
                _helper.Connect();

            return _node;
        }
    }
}
