using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PQHD.Dialog
{
    // ===============================================================
    // Container for a dialoggraph asset
    // ===============================================================
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Dialog Graph", menuName = "PQHD/Dialog Graph", order = 0)]
    public class DialogGraphContainer : ScriptableObject
    {
        public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
        [SerializeReference]
        public List<SerializedNode> nodeData = new List<SerializedNode>();
        public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
    }

    [System.Serializable]
    public class NodeLinkData
    {
        public string baseNodeGuid;
        public string portName;
        public string targetNodeGuid;
    }


    [System.Serializable]
    public class SerializedPort
    {
        public string portName;
        public bool retriggerEnabled;

        public SerializedPort(string portName, bool retriggerEnabled)
        {
            this.portName = portName;
            this.retriggerEnabled = retriggerEnabled;
        }
    }

    [System.Serializable]
    public class SerializedNode
    {
        public string GUID;
        public List<SerializedPort> ports;
        public Vector2 position;
    }


    [System.Serializable]
    public class SerializedDialogNode : SerializedNode
    {
        public string dialogText;
    }

    [System.Serializable]
    public class SerializedRetriggerNode : SerializedNode
    {
        public int outputCount;
    }

    [System.Serializable]
    public class SerializedEventNode : SerializedNode
    {
        public string eventKey;
        public string eventBody;

        public string eventString => eventKey + eventBody;
    }

    [System.Serializable]
    public class SerializedComparisonNode : SerializedNode
    {
        public string propertyName;

        public enum PropertyType
        {
            String,
            Bool,
            Int,
            Float
        }

        public enum Comparison
        {
            Equals,
            Greater,
            Less,
        }

        public PropertyType type;
        public Comparison comparison;

        public string comparator;
    }

    [System.Serializable]
    public class ExposedProperty
    {
        public string PropertyName = "New String";
        public string PropertyValue = "New Value";
    }
}