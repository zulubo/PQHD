using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace PQHD.Dialog
{
    // ===============================================================
    // Dialog graph editor window
    // ===============================================================
    public class DialogGraph : EditorWindow
    {
        [MenuItem("Graph/Dialog Graph")]
        public static void OpenDialogGraphWindow()
        {
            var window = GetWindow<DialogGraph>();
            window.titleContent = new GUIContent("Dialog Graph");
        }

        public DialogGraphContainer target;

        private VisualElement _noGraphView;
        private DialogGraphView _graphView;
        private VisualElement _toolbar;

        private Button _saveButton;
        private Button _helpButton;
        private static EditorWindow _helpWindow;

        private void OnEnable()
        {
            _graphView = null;
            GenerateDialogGraphSelector();

            if (target != null)
            {
                SelectDialogGraphTarget(target);
            }

            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if(target != null && (evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.S)
            {
                Save();
            }
        }

        void GenerateDialogGraphSelector()
        {
            _noGraphView = new Toolbar();

            var createButton = new Button(() => CreateNewDialogGraph());
            createButton.text = "Create New Dialog Graph";
            _noGraphView.Add(createButton);

            rootVisualElement.Add(_noGraphView);
        }

        void CreateNewDialogGraph()
        {
            //string path = EditorUtility.SaveFilePanelInProject("Create Dialog Graph", "New Dialog Graph", "asset", "choose a file name");
            //if (string.IsNullOrEmpty(path)) return; // cancelled

            string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (folderPath.Contains("."))
                folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));

            if (string.IsNullOrEmpty(folderPath)) folderPath = "Assets";

            string path = folderPath + "/New Dialog Graph.asset";

            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var newGraph = ScriptableObject.CreateInstance<DialogGraphContainer>();
            AssetDatabase.CreateAsset(newGraph, path);
            AssetDatabase.Refresh();

            Selection.activeObject = newGraph;
            EditorGUIUtility.PingObject(newGraph);

            SelectDialogGraphTarget(target);
        }

        public void SelectDialogGraphTarget(DialogGraphContainer target)
        {
            this.target = target;

            if (rootVisualElement.Contains(_noGraphView)) rootVisualElement.Remove(_noGraphView);

            ConstructGraphView();
            GenerateToolbar();

            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            saveUtility.LoadGraph(target);
        }

        private void ConstructGraphView()
        {
            _graphView = new DialogGraphView(this)
            {
                name = "Dialog Graph"
            };
            _graphView.StretchToParentSize();
            GenerateMiniMap();
            GenerateBlackboard();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            _toolbar = new Toolbar();

            _saveButton = new Button(() => Save()) { text = "Save Data" };
            _toolbar.Add(_saveButton);

            _toolbar.Add(new ToolbarSpacer());

            _helpButton = new Button(()=> OpenHelpWindow()) { text = "?" };
            _toolbar.Add(_helpButton);

            rootVisualElement.Add(_toolbar);
        }

        private void OnGUI()
        {
            if(_saveButton != null) _saveButton.SetEnabled(_graphView.isDirty);
        }

        private void GenerateMiniMap()
        {
            var minimap = new MiniMap { anchored = true };
            var coords = _graphView.contentViewContainer.WorldToLocal(new Vector2(maxSize.x - 10, 30));
            minimap.SetPosition(new Rect(coords.x, coords.y, 200, 140));
            _graphView.Add(minimap);
        }

        private void GenerateBlackboard()
        {
            var blackboard = new Blackboard();
            blackboard.Add(new BlackboardSection { title = "Exposed Properties" });

            blackboard.addItemRequested = _blackboard => { _graphView.AddPropertyToBlackboard(new ExposedProperty()); };
            blackboard.editTextRequested = (blackboard1, element, newValue) =>
            {
                var oldName = ((BlackboardField)element).text;
                if (_graphView.exposedProperties.Any(x => x.PropertyName == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "Property name already exists", "ok");
                    return;
                }

                var index = _graphView.exposedProperties.FindIndex(x => x.PropertyName == oldName);
                _graphView.exposedProperties[index].PropertyName = newValue;
                ((BlackboardField)element).text = newValue;
            };

            blackboard.SetPosition(new Rect(10, 30, 200, 300));

            _graphView.Add(blackboard);
            _graphView.blackboard = blackboard;
        }

        void Save()
        {
            if (target == null)
            {
                EditorUtility.DisplayDialog("Target graph asset missing", "", "ok");
                return;
            }

            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            saveUtility.SaveGraph(target);
            _graphView.isDirty = false;
        }

        void OpenHelpWindow()
        {
            if(_helpWindow == null)
            {
                _helpWindow = DialogGraphHelp.OpenDialogGraphHelpWindow();
            }
            else
            {
                _helpWindow.Close();
                _helpWindow = null;
            }
        }


        private void OnDisable()
        {
            if (rootVisualElement.Contains(_noGraphView)) rootVisualElement.Remove(_noGraphView);
            if (rootVisualElement.Contains(_graphView)) rootVisualElement.Remove(_graphView);
            if (rootVisualElement.Contains(_toolbar)) rootVisualElement.Remove(_toolbar);
        }


#if UNITY_EDITOR
        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OpenDialogGraph(int instanceID, int line)
        {
            var asset = UnityEditor.EditorUtility.InstanceIDToObject(instanceID);
            if (asset is DialogGraphContainer)
            {
                var window = GetWindow<DialogGraph>();
                if (window == null) window = CreateWindow<DialogGraph>();

                window.rootVisualElement.Clear();

                window.SelectDialogGraphTarget(asset as DialogGraphContainer);

                return true;
            }
            return false;
        }
#endif
    }

    public class DialogGraphHelp : EditorWindow
    {
        public static EditorWindow OpenDialogGraphHelpWindow()
        {
            var window = GetWindow<DialogGraphHelp>();
            window.titleContent = new GUIContent("Dialog Graph Help");
            return window;
        }

        private void OnEnable()
        {
            AddHelpText();
        }

        void AddHelpText()
        {
            var header = new Label("Add inline events to dialog by placing them within [brackets] \n \n Event Types: \n");
            header.style.whiteSpace = WhiteSpace.Normal;
            rootVisualElement.Add(header);

            for (int i = 0; i < DialogManager.inlineDialogEvents.Length; i++)
            {
                var ev = DialogManager.inlineDialogEvents[i];
                VisualElement eventElement = new VisualElement();
                eventElement.style.whiteSpace = WhiteSpace.Normal;
                eventElement.style.flexDirection = FlexDirection.Row;
                var ld = new Label(ev.key + "     " + ev.description + "\n"); ld.style.whiteSpace = WhiteSpace.Normal;
                eventElement.Add(ld);
                rootVisualElement.Add(eventElement);
            }
        }
    }
}