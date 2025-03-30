using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace InspectorGraph
{
    public class PortListView : VisualElement
    {
        private readonly Func<int, Port> _factory;

        private readonly Color _borderColor = new Color(0.07843138f, 0.07843138f, 0.07843138f);

        public int PortCount { get; private set; }

        public PortListView(string label, Func<int, Port> factory)
        {
            SetStyle();

            _factory = factory;

            contentContainer = CreateContainerContainer();

            AddLabel(label);

            VisualElement viewFooter = AddViewFooter();

            Button addButton = AddButton(viewFooter, "d_Toolbar Plus@2x");

            addButton.clicked += () =>
            {
                Port port = AddPort();

                if (Added != null)
                    Added(port);
            };

            Button removeButton = AddButton(viewFooter, "d_Toolbar Minus@2x");

            removeButton.clicked += () =>
            {
                if (PortCount == 0)
                    return;

                Port port = RemovePort();

                if (Removed != null)
                    Removed(port);
            };

            hierarchy.Add(viewFooter);
        }

        private void SetStyle()
        {
            style.marginTop = 2;
            style.marginBottom = 2;
        }

        private void AddLabel(string text)
        {
            var label = new Label(text);

            label.style.marginLeft = 20;
            label.style.height = 24;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;

            contentContainer.Add(label);
        }

        private VisualElement AddViewFooter()
        {
            var viewFooter = new VisualElement();

            viewFooter.AddToClassList("unity-list-view__footer");

            viewFooter.style.borderTopWidth = 1;

            viewFooter.style.borderTopRightRadius = 4;
            viewFooter.style.borderTopLeftRadius = 4;

            viewFooter.style.borderBottomRightRadius = 0;
            viewFooter.style.borderBottomLeftRadius = 0;

            viewFooter.style.marginTop = -22;

            return viewFooter;
        }

        private Button AddButton(VisualElement viewFooter, string icon)
        {
            var button = new Button();
            button.iconImage = (Texture2D)EditorGUIUtility.IconContent(icon).image;
            viewFooter.Add(button);

            button[0].style.marginLeft = 0;
            button[0].style.marginRight = 0;

            button.style.paddingLeft = 2;
            button.style.paddingRight = 2;

            return button;
        }

        private VisualElement CreateContainerContainer()
        {
            var element = new VisualElement();

            element.style.minHeight = 16;

            element.style.SetBorderColor(_borderColor);

            element.style.backgroundColor = new Color(0.2745098f, 0.2745098f, 0.2745098f);

            element.style.marginLeft = 2;
            element.style.marginRight = 2;

            element.style.paddingTop = 1;
            element.style.paddingBottom = 1;

            element.style.SetBorderWidth(1);
            element.style.SetBorderRadius(4);

            hierarchy.Add(element);

            return element;
        }

        public override VisualElement contentContainer { get; }


        // These events are only raised, when a port is added through the UI.

        public event Action<Port> Added;
        public event Action<Port> Removed;

        private void SetPortStyle(Port port)
        {
            port.style.paddingLeft = 1;
        }

        public Port AddPort()
        {
            Port port = _factory(PortCount);

            SetPortStyle(port);

            PortCount++;

            contentContainer.Add(port);

            return port;
        }

        public Port RemovePort()
        {
            // On issue here is, that when removing a port, the edges still persist. For now, I don't mind though.

            var port = (Port)contentContainer[PortCount];

            contentContainer.RemoveAt(PortCount);

            PortCount -= 1;

            return port;
        }
    }
}