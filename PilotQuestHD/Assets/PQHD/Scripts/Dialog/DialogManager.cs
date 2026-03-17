using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Audio;

namespace PQHD.Dialog 
{
    // ===============================================================
    // Singleton that draws dialog boxes and handles interaction with them
    // ===============================================================
    public class DialogManager : MonoBehaviour
    {
        public static DialogManager I;
        public static bool IsPlayingDialog => I.activeDialog != null;
        public static DialogGraphRuntime ActiveDialogRuntime => IsPlayingDialog ? I.activeDialog.runtime : null;

        [System.Serializable]
        private class DialogBox
        {
            public GameObject gameObject;
            public RectTransform transform;
            public RectTransform visual;
            public TMP_Text textbox;
            public CanvasGroup portGroup;
            public CanvasGroup defaultContinue;
            
            [SerializeField] private RectTransform portPointer;

            [SerializeField] private Color portTextColorUnselected = Color.white;
            [SerializeField] private Color portTextColorSelected = Color.yellow;

            public struct Port
            {
                public GameObject gameObject;
                public TMP_Text text;
                public RectTransform transform;
                public string name;
            }
            public List<Port> ports = new List<Port>();

            private int oldNavValue;
            public int SelectedPort { get; private set; }

            public void AddPort(GameObject gameObject, string name)
            {
                TMP_Text text = gameObject.GetComponentInChildren<TMP_Text>();
                text.text = name;
                ports.Add(new Port()
                {
                    gameObject = gameObject,
                    text = text,
                    transform = gameObject.GetComponent<RectTransform>(),
                    name = name,
                });
            }

            public void ClearPorts()
            {
                for (int i = 0; i < ports.Count; i++)
                {
                    if(ports[i].gameObject) Destroy(ports[i].gameObject);
                }
                ports.Clear();
            }
            
            public bool IsOpen => gameObject.activeSelf;

            public void Open()
            {
                gameObject.SetActive(true);
            }

            public void Close()
            {
                gameObject.SetActive(false);
            }

            public void UpdateNavigation(Vector2 navInput)
            {
                int navValue = 0;
                if(navInput.y < -0.8f) navValue = 1;
                else if(navInput.y > 0.8f) navValue = -1;

                if (navValue != oldNavValue)
                {
                    SelectPort(SelectedPort + navValue);
                    oldNavValue = navValue;
                }
            }

            public void SelectPort(int port)
            {
                if (ports.Count == 0) return;
                
                if (port < 0) port = 0;
                else if(port >= ports.Count) port = ports.Count - 1;
                SelectedPort = port;
                portPointer.anchoredPosition = ports[port].transform.anchoredPosition;

                for (int p = 0; p < ports.Count; p++)
                {
                    ports[p].text.color = p == SelectedPort ? portTextColorSelected : portTextColorUnselected;
                }
            }
        }

        [SerializeField] DialogBox dialogBox;

        public GameObject portButtonPrefab;
        
        public float layout_margin = 100;
        public float layout_height = 300;

        public float charactersPerSecond = 30;

        private void Awake()
        {
            I = this;
        }


        public delegate void DialogFinishEvent(bool cancel = false, string selectedPort = "DEFAULT");

        public static void TriggerDialog(SerializedDialogNode node, DialogGraphRuntime runtime, List<string> choices, DialogFinishEvent onFinish)
        {
            if(I == null)
            {
                Debug.LogError("No DialogManager in scene. Add it to the GameManager.");
                return;
            }

            I.ShowDialogBox(node, runtime, choices, onFinish);
        }

        private class ActiveDialog
        {
            public Coroutine coroutine;
            public DialogGraphRuntime runtime;
            public DialogFinishEvent finishEvent;
            public bool finishedReading = false;
            public int startFrame;
            public bool hasPorts;
            public bool skip;
        }

        ActiveDialog activeDialog;
        private void ShowDialogBox(SerializedDialogNode node, DialogGraphRuntime runtime, List<string> choices, DialogFinishEvent onFinish)
        {
            if(activeDialog != null)
            {
                StopCoroutine(activeDialog.coroutine);
                activeDialog.finishEvent(true); // dialog has been forcibly closed
                activeDialog = null;
            }

            activeDialog = new ActiveDialog() { finishEvent = onFinish, runtime = runtime };
            activeDialog.coroutine = StartCoroutine(DialogBoxCoroutine(node, choices)); // separated so coroutine can access activeDialogBox
        }

        private const float FadeDuration = 0.15f;

        private IEnumerator DialogBoxCoroutine(SerializedDialogNode node, List<string> choices)
        {
            dialogBox.Open();
/*
            // hardcoded to place in lower third for now -- add options for top too later
            dialogBoxInstance.transform.anchorMin = new Vector2(0, 0);
            dialogBoxInstance.transform.anchorMax = new Vector2(1, 0);
            dialogBoxInstance.transform.sizeDelta = new Vector2(-layout_margin * 2, layout_height);
            dialogBoxInstance.transform.anchoredPosition = new Vector2(0, layout_margin + layout_height / 2f);
*/
            // set up dialog branch buttons if needed
            activeDialog.hasPorts = choices != null && choices.Count > 0;// node.ports.Count > 1 || node.ports[0].portName != "DEFAULT";
            dialogBox.portGroup.gameObject.SetActive(activeDialog.hasPorts);
            dialogBox.defaultContinue.gameObject.SetActive(!activeDialog.hasPorts);
            dialogBox.ClearPorts();
            if(activeDialog.hasPorts)
            {
                for (int i = 0; i < choices.Count; i++)
                {
                    string c = choices[i];
                    GameObject portInst = Instantiate(portButtonPrefab, dialogBox.portGroup.transform);
                    portInst.SetActive(true);
                    dialogBox.AddPort(portInst, c);
                }
            }
            var exitGroup = activeDialog.hasPorts ? dialogBox.portGroup : dialogBox.defaultContinue;
            exitGroup.alpha = 0;
            exitGroup.interactable = false;

            // parse events
            string dialogText = ParseInlineEvents(node.dialogText, out List<ParsedInlineEvent> events);
            List<ParsedInlineEvent> eventsWithDelay = events.FindAll(e => e.delay > 0);
            float totalDelays = 0; eventsWithDelay.ForEach(e => totalDelays += e.delay);
            
            // parse inline properties
            dialogText = ParseInlineProperties(node.dialogText, activeDialog.runtime);

            // precalculate some stuff about the length of the dialog text
            dialogBox.textbox.text = dialogText;
            dialogBox.textbox.ForceMeshUpdate(true, true);
            var parsedText = dialogBox.textbox.GetParsedText();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialogBox.textbox.rectTransform.parent.parent);
            
            float textWindowHeight = ((RectTransform)dialogBox.textbox.rectTransform.parent).rect.height;
            float scrollAmount = dialogBox.textbox.rectTransform.rect.height - textWindowHeight;
            dialogBox.textbox.rectTransform.anchoredPosition = Vector2.zero;

            dialogBox.textbox.maxVisibleCharacters = 0;
            
            activeDialog.startFrame = Time.frameCount;

            if (!wasDialogOpenLastFrame)
            {
                // animate box opening
                Rect boxRect = ((RectTransform)dialogBox.visual.parent).rect;
                dialogBox.visual.sizeDelta = new Vector2(-boxRect.width, 0);
                float open = 0;
                while (open < 1)
                {
                    open += Time.deltaTime / 0.2f;
                    float openCurved = (1 - open) * (1 - open);
                    dialogBox.visual.sizeDelta = new Vector2(-boxRect.width * openCurved, 0);
                    if (activeDialog.skip) break;
                    yield return null;
                }
            }

            dialogBox.visual.sizeDelta = Vector2.zero;

            // animate text appearing
            activeDialog.finishedReading = false;
            float duration = parsedText.Length / charactersPerSecond;

            float t = 0;
            // iterate through events so they can be triggered as the text is appearing
            for (int i = 0; i < events.Count + 1; i++)
            {
                if (!activeDialog.skip)
                {
                    float tpos = i < events.Count ? (float)events[i].position / (float)parsedText.Length : 1f;
                    while (t < tpos)
                    {
                        if (activeDialog.skip) break;

                        t += Time.deltaTime / duration;
                        dialogBox.textbox.maxVisibleCharacters = Mathf.RoundToInt(t * parsedText.Length);
                        
                        if(activeDialog.runtime.voice) PlayVoice(activeDialog.runtime.voice);

                        if (scrollAmount > 0)
                        {
                            // scroll through text if it's long enough
                            float scrollPos =
                                Mathf.Clamp01(Mathf.Lerp(-textWindowHeight / 2f, scrollAmount + textWindowHeight / 2f,
                                    t) / scrollAmount) * scrollAmount;
                            dialogBox.textbox.rectTransform.anchoredPosition = new Vector2(0, scrollPos);
                        }

                        yield return null;
                    }
                }

                if (i < events.Count)
                {
                    events[i].sourceEvent.output?.Invoke(activeDialog.runtime, events[i].eventParams);
                    yield return new WaitForSeconds(events[i].delay);
                }
            }

            // done revealing text
            activeDialog.finishedReading = true;
            dialogBox.textbox.maxVisibleCharacters = parsedText.Length;
            float endScrollPos = Mathf.Clamp01((scrollAmount + textWindowHeight / 2f) / scrollAmount) * scrollAmount;
            dialogBox.textbox.rectTransform.anchoredPosition = new Vector2(0, endScrollPos);
            yield return null;
            
            if (activeDialog.hasPorts)
            {
                if (node.defaultPort > 0 && node.defaultPort < node.ports.Count)
                {
                    dialogBox.SelectPort(node.defaultPort);
                }
                else
                {
                    dialogBox.SelectPort(0);
                }
            }

            // fade in exit options
            float fade = 0;
            while(fade < 1)
            {
                fade += Time.deltaTime / FadeDuration;
                exitGroup.alpha = Mathf.SmoothStep(0, 1, fade);
                yield return null;
            }
            exitGroup.alpha = 1;
            exitGroup.interactable = true;
        }

        private void Update()
        {
            if (activeDialog != null)
            {
                if (activeDialog.hasPorts)
                {
                    dialogBox.UpdateNavigation(Input.MoveAxis.Position);
                }

                if (Input.ButtonA.WasPressedThisFrame || Input.ButtonB.WasPressedThisFrame)
                {
                    UISubmit();
                }
            }
        }

        private bool wasDialogOpenLastFrame;
        private void LateUpdate()
        {
            wasDialogOpenLastFrame = IsPlayingDialog;
        }

        // clicked on one of the dialog branch options
        void ExecutePort(string portName)
        {
            StartCoroutine(DelayAndSelectPort(portName));
        }
        // delay one frame to input from registering
        IEnumerator DelayAndSelectPort(string portName)
        {
            yield return null;
            if (activeDialog == null || !activeDialog.finishedReading) yield break;
            var box = activeDialog;
            activeDialog = null;
            StopCoroutine(box.coroutine);
            dialogBox.Close();
            box.finishEvent(false, portName);
        }

        // clicked the submit button, continues when there are no branches
        private void UISubmit()
        {
            if (activeDialog == null) return;
            if (!activeDialog.finishedReading && Time.frameCount > activeDialog.startFrame)
            {
                activeDialog.skip = true;
                return;
            }
            
            if (activeDialog.hasPorts)
            {
                ExecutePort(dialogBox.ports[dialogBox.SelectedPort].name);
            }
            else
            {
                ExecutePort("DEFAULT");
            }
        }


        /// <summary>
        /// Inline dialog events are marked with brackets
        /// </summary>
        public class InlineDialogEvent
        {
            /// <summary>
            /// string that identifies event type
            /// </summary>
            public string key;
            /// <summary>
            /// A description that describes what the event does
            /// </summary>
            public string description;
            /// <summary>
            /// Whether the event should pause dialog readback
            /// </summary>
            public bool pauseDialog = false;
            public delegate void InlineEvent(MonoBehaviour context, string param);
            public delegate float InlineTime(string param);
            public InlineEvent output;
            public InlineTime delayTime = (string param) => 0 ;
        }

        public static readonly InlineDialogEvent[] inlineDialogEvents = new InlineDialogEvent[]
        {
            new InlineDialogEvent()
            {
                key = "_",
                description = "pause denoted by number of underscores",
                pauseDialog = true,
                delayTime = (string param) => param.ToCharArray().ToList().FindAll(c => c == '_').Count * 0.2f + 0.2f
            },

            new InlineDialogEvent()
            {
                key = "custom:",
                description = "custom external event. Requires DialogEventRelay on context behaviour",
                pauseDialog = false,
                output = (MonoBehaviour context, string param) => CustomEvent(context, param)
            },
        };


        static void CustomEvent(MonoBehaviour context, string param)
        {
            var relay = context?.GetComponent<DialogEventRelay>();
            if(relay != null)
            {
                relay.TriggerEvent(param);
            }
            else 
            { 
                Debug.LogWarning("No dialogEventRelay, custom dialog event will not fire: " + param, context);
                return;
            }
        }

        public class ParsedInlineEvent
        {
            /// <summary>
            /// The event to invoke
            /// </summary>
            public InlineDialogEvent sourceEvent;
            /// <summary>
            /// The parameters to pass more data to the event
            /// </summary>
            public string eventParams;
            /// <summary>
            /// How long dialog text playback should pause during this event
            /// </summary>
            public float delay;
            /// <summary>
            /// position in larger text, used to trigger at the right time during dialog
            /// </summary>
            public int position;

            public ParsedInlineEvent(InlineDialogEvent sourceEvent, string eventParams)
            {
                this.sourceEvent = sourceEvent;
                this.eventParams = eventParams;
                delay = sourceEvent.delayTime(eventParams);
            }
        }

        readonly Regex RegexEvents = new Regex(@"\[([^]]*)\]");
        string ParseInlineEvents(string dialog, out List<ParsedInlineEvent> events)
        {
            // use regex to find events contained in brackets
            var foundEvents = RegexEvents.Matches(dialog);

            events = new List<ParsedInlineEvent>();

            int trimmedCount = 0;
            for (int i = 0; i < foundEvents.Count; i++)
            {
                string eventString = foundEvents[i].Value.Substring(1, foundEvents[i].Length - 2);

                if (ParseEvent(eventString, out ParsedInlineEvent parsed))
                {
                    parsed.position = foundEvents[i].Index - trimmedCount; // save position in text sans-events
                    events.Add(parsed);
                }

                trimmedCount += foundEvents[i].Length;
            }

            // remove events from dialog text
            dialog = RegexEvents.Replace(dialog, "");

            return dialog;
        }

        public static bool ParseEvent(string eventString, out ParsedInlineEvent result)
        {
            InlineDialogEvent foundEvent = System.Array.Find(inlineDialogEvents, e => eventString.StartsWith(e.key));
            if (foundEvent != null)
            {
                // found event matching what's in the brackets
                eventString = eventString.Remove(0, foundEvent.key.Length);
                result = new ParsedInlineEvent(foundEvent, eventString);
                return true;
            }
            else
            {
                result = null;
                Debug.LogError("Unknown Dialog Event: " + eventString);
                return false;
            }
        }
        
        readonly Regex RegexProps = new Regex(@"\<prop:([^]]*)\>");
        string ParseInlineProperties(string dialog, DialogGraphRuntime runtime)
        {
            // use regex to find properties contained in angle brackets
            var foundEvents = RegexProps.Matches(dialog);

            int trimmedCount = 0;
            for (int i = foundEvents.Count - 1; i >= 0; i--)
            {
                string propString = foundEvents[i].Groups[1].Value;

                if (!runtime.GetProperty(propString, out string propValue))
                {
                    propValue = "<ERROR PROPERTY NOT FOUND>";
                }
                
                // replace with property value
                dialog = dialog.Remove(foundEvents[i].Index, foundEvents[i].Length).Insert(foundEvents[i].Index, propValue);
            }

            return dialog;
        }

        private Audio.PlayingSound voicePlaying;

        private void PlayVoice(AudioResource voice)
        {
            if (voicePlaying != null && !voicePlaying.finished) return;
            voicePlaying = Audio.I.PlaySound2D(voice);
        }
    }
}