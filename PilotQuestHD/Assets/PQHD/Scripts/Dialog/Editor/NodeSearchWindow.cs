using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace PQHD.Dialog
{
    // ===============================================================
    // Node search window for dialog graphs
    // ===============================================================
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogGraphView _graphView;
        private EditorWindow _window;
        public void Init(DialogGraphView graph, EditorWindow window)
        {
            _graphView = graph;
            _window = window;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeEntry(new GUIContent("Dialog Node"))
            {
                userData = new DialogNode(), level = 1
            },
            new SearchTreeEntry(new GUIContent("Retrigger Node"))
            {
                userData = new RetriggerNode(), level = 1
            },
            new SearchTreeEntry(new GUIContent("Event Node"))
            {
                userData = new EntryNode(), level = 1
            },
            new SearchTreeEntry(new GUIContent("Comparison Node"))
            {
                userData = new ComparisonNode(), level = 1
            }
        };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);

            if (SearchTreeEntry.userData is DialogNode)
            {
                _graphView.CreateDialogNode("Dialog Node", localMousePosition);
                return true;
            }
            else if (SearchTreeEntry.userData is RetriggerNode)
            {
                _graphView.CreateRetriggerNode(localMousePosition);
                return true;
            }
            else if (SearchTreeEntry.userData is EntryNode)
            {
                _graphView.CreateEventNode(localMousePosition);
                return true;
            }
            else if (SearchTreeEntry.userData is ComparisonNode)
            {
                _graphView.CreateComparisonNode(localMousePosition);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}