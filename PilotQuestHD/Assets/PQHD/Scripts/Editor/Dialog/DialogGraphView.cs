using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PQHD.Dialog
{
    // ===============================================================
    // Dialog Graph editor using unity's GraphView
    // ===============================================================
    public class DialogGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

        public Blackboard blackboard;
        public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();

        private NodeSearchWindow _searchWindow;

        public bool isDirty = false;

        public DialogGraphView(EditorWindow editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);

            AddElement(EntryNode.Create(this));

            AddSearchWindow(editorWindow);
            graphViewChanged = OnGraphChange;

            serializeGraphElements = Copy;
            canPasteSerializedData = CanPaste;
            unserializeAndPaste = Paste;
        }

        private void AddSearchWindow(EditorWindow editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Init(this, editorWindow);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition, 0, 0), _searchWindow);
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatible = new List<Port>();
            ports.ForEach((port) => { if (startPort != port && startPort.node != port.node) compatible.Add(port); });
            return compatible;
        }

        public static Port GeneratePort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(int));
        }

        public void CreateDialogNode(string dialogText, Vector2 position)
        {
            var node = DialogNode.Create(this, position);
            node.dialogText.value = dialogText;
            AddElement(node);
            isDirty = true;
        }

        public void CreateRetriggerNode(Vector2 position)
        {
            AddElement(RetriggerNode.Create(this, position));
            isDirty = true;
        }

        public void CreateEventNode(Vector2 position)
        {
            AddElement(EventNode.Create(this, position));
            isDirty = true;
        }

        public void CreateComparisonNode(Vector2 position)
        {
            AddElement(ComparisonNode.Create(this, position));
            isDirty = true;
        }

        public static VisualElement Spacer(int size)
        {
            var s = new Label("");
            s.style.height = size;
            s.style.width = size;
            return s;
        }


        public void AddPropertyToBlackboard(ExposedProperty exposedProperty)
        {
            var localPropertyName = exposedProperty.PropertyName;
            var localPropertyValue = exposedProperty.DefaultValue;
            while (exposedProperties.Any(x => x.PropertyName == localPropertyName))
                localPropertyName = $"{localPropertyName}(1)";

            var property = new ExposedProperty();
            property.PropertyName = localPropertyName;
            property.DefaultValue = localPropertyValue;
            exposedProperties.Add(property);

            var container = new VisualElement();
            var field = new BlackboardField { text = property.PropertyName, typeText = "" };
            container.Add(field);

            var propertyValueTextField = new TextField("Default Value")
            {
                value = localPropertyValue
            };
            propertyValueTextField.RegisterValueChangedCallback(evt =>
            {
                var i = exposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);
                exposedProperties[i].DefaultValue = evt.newValue;
            });

            var valueRow = new BlackboardRow(field, propertyValueTextField);
            container.Add(valueRow);

            blackboard.Add(container);
            isDirty = true;
        }

        public void ClearBlackboardAndExposedProperties()
        {
            exposedProperties.Clear();
            blackboard.Clear();
            isDirty = true;
        }

        public static void AddDefaultPort(Node node)
        {
            var generatedPort = GeneratePort(node, Direction.Output);

            generatedPort.portName = "DEFAULT";

            node.outputContainer.Add(generatedPort);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        public void ResetPorts(Node node)
        {
            var ports = node.outputContainer.Children().ToList();
            ports.ForEach(p => 
            {
                if (p is Port) RemovePort(node, p as Port);
            });
        }

        public void RemovePort(Node node, Port generatedPort)
        {
            var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }
            node.outputContainer.Remove(generatedPort);

            if (node.outputContainer.childCount == 0)
            {
                // add default port when all choices removed
                AddDefaultPort(node);
            }

            node.RefreshPorts();
            node.RefreshExpandedState();
            isDirty = true;
        }

        private GraphViewChange OnGraphChange(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                foreach (GraphElement e in change.elementsToRemove)
                {
                    if (e is BlackboardField)
                    {
                        //actually delete the blackboard field
                        var bf = (BlackboardField)e;
                        //blackboard.Remove(bf.parent);
                        var property = exposedProperties.Find(x => x.PropertyName == bf.text);
                        exposedProperties.Remove(property);
                    }
                }
            }
            isDirty = true;
            return change;
        }

        List<GraphElement> copyElements;
        string Copy(IEnumerable<GraphElement> elements)
        {
            copyElements = elements.ToList();
            return "Copy Nodes";
        }

        bool CanPaste(string data)
        {
            return data == "Copy Nodes";
        }

        void Paste(string operation, string data)
        {
            if (copyElements == null) return;

            Dictionary<string, string> newGUIDs = new Dictionary<string, string>();

            var copiedNodes = copyElements.Where(e => e is DialogNodeBase).Cast<DialogNodeBase>().ToList();

            Vector2 offset = new Vector2(100, 100);

            List<DialogNodeBase> pastedNodes = new List<DialogNodeBase>();

            foreach (DialogNodeBase node in copiedNodes)
            {
                DialogNodeBase newNode = null;
                SerializedNode nodeData = null;
                if (node is EventNode)
                {
                    var en = node as EventNode;
                    nodeData = en.SerializeNode();
                    newNode = EventNode.Create(this, Vector2.zero);
                }
                if (node is DialogNode)
                {
                    var dn = node as DialogNode;
                    nodeData = dn.SerializeNode();
                    newNode = DialogNode.Create(this, Vector2.zero);
                }
                if (node is RetriggerNode)
                {
                    var rn = node as RetriggerNode;
                    nodeData = rn.SerializeNode();
                    newNode = RetriggerNode.Create(this, Vector2.zero);
                }

                if (newNode == null || nodeData == null) continue;

                nodeData.position += offset;
                string newGUID = Guid.NewGuid().ToString();
                newGUIDs.Add(node.GUID, newGUID);
                nodeData.GUID = newGUID;
                newNode.DeserializeNode(this, nodeData);
                pastedNodes.Add(newNode);
                AddElement(newNode);
            };

            var copiedLinks = copyElements.Where(e => e is Edge).Cast<Edge>().ToList();

            foreach(Edge link in copiedLinks) 
            {
                if (!newGUIDs.ContainsKey((link.output.node as DialogNodeBase).GUID) ||
                    !newGUIDs.ContainsKey((link.input.node as DialogNodeBase).GUID)) continue;
                var outputNode = pastedNodes.Find(n => n.GUID == newGUIDs[(link.output.node as DialogNodeBase).GUID]);
                var inputNode = pastedNodes.Find(n => n.GUID == newGUIDs[(link.input.node as DialogNodeBase).GUID]);

                var outputPort = outputNode.outputContainer.Children().ToList().Find(p => p is Port && ((Port)p).portName == link.output.portName) as Port;
                var inputPort = inputNode.inputContainer.Children().ToList().Find(p => p is Port && ((Port)p).portName == link.input.portName) as Port;

                var tempEdge = new Edge
                {
                    output = outputPort,
                    input = inputPort
                };

                tempEdge.input.Connect(tempEdge);
                tempEdge.output.Connect(tempEdge);
                AddElement(tempEdge);
            }
            isDirty = true;
        }
    }
}