using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PQHD.Dialog
{
    // ===============================================================
    // Evaluates a dialog graph, aka runs a conversation
    // ===============================================================
    public class DialogGraphRuntime : MonoBehaviour
    {
        public DialogGraphContainer dialog;
        CachedNode activeNode = null;

        [HideInInspector]
        public int playCount;
        [Tooltip("Maximum number of times you can start this dialog")]
        public int maximumPlayCount = -1;


        // store choices we make so we can hide them upon replaying if needed
        [HideInInspector]
        public Dictionary<string, List<string>> triggeredChoices = new Dictionary<string, List<string>>();
        private void RegisterChoice(string node, string choice)
        {
            if (!triggeredChoices.ContainsKey(node))
            {
                triggeredChoices[node] = new List<string>();
            }

            if (!triggeredChoices[node].Contains(choice))
            {
                triggeredChoices[node].Add(choice);
            }
        }
        private bool HasMadeChoice(string node, string choice)
        {
            return triggeredChoices.ContainsKey(node) && triggeredChoices[node].Contains(choice);
        }

        public bool isPlaying { get; private set; }

        // cache extra data about node for efficiency
        class CachedPort
        {
            public string portName;
            public CachedNode connectedNode;
        }
        class CachedNode
        {
            public SerializedNode node;
            public List<CachedPort> ports;
            public bool GetPort(string name, out CachedPort port)
            {
                port = ports.Find(p => p.portName == name);
                return port != null;
            }
        }

        List<CachedNode> cachedGraph;

        private void Start()
        {
            CacheGraph();
        }

        void CacheGraph()
        {
            cachedGraph = new List<CachedNode>();
            foreach(SerializedNode node in dialog.nodeData)
            {
                var cached = new CachedNode() { node = node };
                cachedGraph.Add(cached);
            }

            for (int i = 0; i < cachedGraph.Count; i++)
            {
                List<CachedPort> ports = new List<CachedPort>();
                dialog.nodeLinks.ForEach(l => { if (l.baseNodeGuid == cachedGraph[i].node.GUID) ports.Add(new CachedPort() { portName = l.portName, connectedNode = cachedGraph.Find(n => n.node.GUID == l.targetNodeGuid) }); });
                cachedGraph[i].ports = ports;
            }
        }

        public void Play()
        {
            if (isPlaying || DialogManager.IsPlayingDialog) return;

            isPlaying = true;
            playCount++;

            // transition to first node connected to start node
            activeNode = cachedGraph[0];
            NodeFinished(false, "DEFAULT");
        }

        void EvaluateNode(CachedNode node)
        {
            activeNode = node;
            if(node.node is SerializedDialogNode)
            {
                EvaluateDialogNode(node.node as SerializedDialogNode);
            }
            if(node.node is SerializedRetriggerNode)
            {
                EvaluateRetriggerNode(node.node as SerializedRetriggerNode);
            }
            if (node.node is SerializedEventNode)
            {
                EvaluateEventNode(node.node as SerializedEventNode);
            }

            if (node.node is SerializedComparisonNode)
            {
                EvaluateComparisonNode(node.node as SerializedComparisonNode);
            }
        }

        void EvaluateDialogNode(SerializedDialogNode dialogNode)
        {
            List<string> choices = new List<string>();
            for (int i = 0; i < dialogNode.ports.Count; i++)
            { // make sure each choice is valid and should be shown on dialog box
                if (dialogNode.ports[i] != null && dialogNode.ports[i].portName != "DEFAULT" 
                    && !(!dialogNode.ports[i].retriggerEnabled && HasMadeChoice(dialogNode.GUID, dialogNode.ports[i].portName)))
                {
                    choices.Add(dialogNode.ports[i].portName);
                }
            }
            if (choices.Count == 0) choices = null;
            DialogManager.TriggerDialog(dialogNode, this, choices, NodeFinished);
        }

        void EvaluateRetriggerNode(SerializedRetriggerNode retriggerNode)
        {
            if(playCount <= retriggerNode.outputCount)
            {
                NodeFinished(false, playCount.ToString());
            }
            else
            {
                NodeFinished(false, "DEFAULT");
            }
        }

        void EvaluateEventNode(SerializedEventNode eventNode)
        {
            if(DialogManager.ParseEvent(eventNode.eventString, out DialogManager.ParsedInlineEvent parsedEvent))
            {
                parsedEvent.sourceEvent.output?.Invoke(this, parsedEvent.eventParams);
            }
            NodeFinished(false, "DEFAULT");
        }
        
        void EvaluateComparisonNode(SerializedComparisonNode eventNode)
        {
            ExposedProperty prop = dialog.exposedProperties.Find(p => p.PropertyName == eventNode.propertyName);
            if (prop == null)
            {
                Debug.LogError($"Property {eventNode.propertyName} in comparison node not found", dialog);
                return;
            }

            bool result = false;

            switch (eventNode.type)
            {
                case SerializedComparisonNode.PropertyType.String:
                    result = prop.PropertyValue == eventNode.comparator;
                    break;
                case SerializedComparisonNode.PropertyType.Bool:
                    result = bool.Parse(prop.PropertyValue) == bool.Parse(eventNode.comparator);
                    break;
                case SerializedComparisonNode.PropertyType.Int:
                {
                    int a = int.Parse(prop.PropertyValue);
                    int b = int.Parse(eventNode.comparator);
                    switch (eventNode.comparison)
                    {
                        case SerializedComparisonNode.Comparison.Equals:
                            result = a == b;
                            break;
                        case SerializedComparisonNode.Comparison.Greater:
                            result = a > b;
                            break;
                        case SerializedComparisonNode.Comparison.Less:
                            result = a < b;
                            break;
                    }

                    break;
                }
                case SerializedComparisonNode.PropertyType.Float:
                {
                    float a = float.Parse(prop.PropertyValue);
                    float b = float.Parse(eventNode.comparator);
                    switch (eventNode.comparison)
                    {
                        case SerializedComparisonNode.Comparison.Equals:
                            result = Mathf.Approximately(a, b);
                            break;
                        case SerializedComparisonNode.Comparison.Greater:
                            result = a > b;
                            break;
                        case SerializedComparisonNode.Comparison.Less:
                            result = a < b;
                            break;
                    }
                    break;
                }
            }

            NodeFinished(false, result ? "True" : "False");
        }

        void NodeFinished(bool canceled, string choice)
        {
            if (canceled)
            {
                StopPlaying();
                return;
            }
            else
            {
                if(activeNode != null) RegisterChoice(activeNode.node.GUID, choice);
                if (activeNode.GetPort(choice, out CachedPort port) && port.connectedNode != null)
                {
                    // transition to next node
                    EvaluateNode(port.connectedNode);
                }
                else
                {
                    StopPlaying();
                }
            }
        }

        void StopPlaying()
        {
            isPlaying = false;
        }
    }
}