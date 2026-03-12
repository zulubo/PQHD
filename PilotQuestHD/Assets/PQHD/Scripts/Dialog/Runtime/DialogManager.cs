using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System.Linq;
using System.Text.RegularExpressions;

namespace PQHD.Dialog 
{
    // ===============================================================
    // Singleton that draws dialog boxes and handles interaction with them
    // ===============================================================
    public class DialogManager : MonoBehaviour
    {
        public static DialogManager I;
        public static bool IsPlayingDialog => I.activeDialogBox != null;


        [System.Serializable]
        private class DialogBox
        {
            public GameObject gameObject;
            public RectTransform transform;
            public TMP_Text textbox;
            public CanvasGroup ports;
            public CanvasGroup defaultContinue;

            private List<Button> portButtons = new List<Button>();

            public void AddPort(GameObject button, string name, UnityEngine.Events.UnityAction onClick)
            {
                button.GetComponentInChildren<TMP_Text>().text = name;
                Button b = button.GetComponent<Button>();
                b.onClick.AddListener(onClick);
                portButtons.Add(b);
            }

            public void ClearPorts()
            {
                for (int i = 0; i < portButtons.Count; i++)
                {
                    if(portButtons[i] != null)
                        Destroy(portButtons[i].gameObject);
                }
                portButtons.Clear();
            }

            public void SelectFirstPortButton()
            {
                portButtons[0].Select();
            }

            public void Open()
            {
                gameObject.SetActive(true);
            }

            public void Close()
            {
                gameObject.SetActive(false);
            }
        }

        [SerializeField] DialogBox dialogBox;

        public GameObject portButtonPrefab;
        
        public float layout_margin = 100;
        public float layout_height = 300;

        public float charactersPerSecond = 30;

        private Camera actorCam;
        Quaternion actorCamRot;
        float actorCamFOV;
        private RenderTexture actorRT;
        public int actorRenderLayer = 31;

        private void Awake()
        {
            I = this;
        }

        private void Start()
        {
            
        }

        public delegate void DialogFinishEvent(bool cancel = false, string selectedPort = "DEFAULT");

        public static void TriggerDialog(SerializedDialogNode node, MonoBehaviour context, List<string> choices, DialogFinishEvent onFinish)
        {
            if(I == null)
            {
                Debug.LogError("No DialogManager in scene. Add it to the GameManager.");
                return;
            }

            I.ShowDialogBox(node, context, choices, onFinish);
        }

        private class ActiveDialogBox
        {
            public Coroutine coroutine;
            public MonoBehaviour context;
            public DialogFinishEvent finishEvent;
            public bool finishedReading = false;
            public bool hasPorts;
        }

        ActiveDialogBox activeDialogBox;
        private void ShowDialogBox(SerializedDialogNode node, MonoBehaviour context, List<string> choices, DialogFinishEvent onFinish)
        {
            if(activeDialogBox != null)
            {
                StopCoroutine(activeDialogBox.coroutine);
                activeDialogBox.finishEvent(true); // dialog has been forcibly closed
                activeDialogBox = null;
            }

            activeDialogBox = new ActiveDialogBox() { finishEvent = onFinish, context = context };
            activeDialogBox.coroutine = StartCoroutine(DialogBoxCoroutine(node, choices, onFinish)); // separated so coroutine can access activeDialogBox
        }

        private IEnumerator DialogBoxCoroutine(SerializedDialogNode node, List<string> choices, DialogFinishEvent onFinish)
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
            activeDialogBox.hasPorts = choices != null && choices.Count > 0;// node.ports.Count > 1 || node.ports[0].portName != "DEFAULT";
            dialogBox.ports.gameObject.SetActive(activeDialogBox.hasPorts);
            dialogBox.defaultContinue.gameObject.SetActive(!activeDialogBox.hasPorts);
            dialogBox.ClearPorts();
            if(activeDialogBox.hasPorts)
            {
                for (int i = 0; i < choices.Count; i++)
                {
                    string c = choices[i];
                    dialogBox.AddPort(Instantiate(portButtonPrefab, dialogBox.ports.transform), c, delegate { SelectPort(c); });
                }
            }
            var exitGroup = activeDialogBox.hasPorts ? dialogBox.ports : dialogBox.defaultContinue;
            exitGroup.alpha = 0;
            exitGroup.interactable = false;

            // parse events
            string dialogText = ParseInlineEvents(node.dialogText, out List<ParsedInlineEvent> events);
            List<ParsedInlineEvent> eventsWithDelay = events.FindAll(e => e.delay > 0);
            float totalDelays = 0; eventsWithDelay.ForEach(e => totalDelays += e.delay);


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

            // animate text appearing
            activeDialogBox.finishedReading = false;
            float duration = parsedText.Length / charactersPerSecond;

            float t = 0;
            // iterate through events so they can be triggered as the text is appearing
            for (int i = 0; i < events.Count + 1; i++)
            {
                float tpos = i < events.Count ? (float)events[i].position / (float)parsedText.Length : 1f;
                while (t < tpos)
                {
                    t += Time.deltaTime / duration;
                    dialogBox.textbox.maxVisibleCharacters = Mathf.RoundToInt(t * parsedText.Length);

                    if (scrollAmount > 0)
                    {
                        // scroll through text if it's long enough
                        float scrollPos = Mathf.Clamp01(Mathf.Lerp(-textWindowHeight / 2f, scrollAmount + textWindowHeight / 2f, t) / scrollAmount) * scrollAmount;
                        dialogBox.textbox.rectTransform.anchoredPosition = new Vector2(0, scrollPos);
                    }

                    yield return null;
                }

                if (i < events.Count)
                {
                    events[i].sourceEvent.output?.Invoke(activeDialogBox.context, events[i].eventParams);
                    yield return new WaitForSeconds(events[i].delay);
                }
            }
            

            // done revealing text
            activeDialogBox.finishedReading = true;
            dialogBox.textbox.maxVisibleCharacters = parsedText.Length;
            yield return null;

            // fade in exit options
            float fade = 0;
            while(fade < 1)
            {
                fade += Time.deltaTime / 0.5f;
                exitGroup.alpha = Mathf.SmoothStep(0, 1, fade);
                yield return null;
            }
            exitGroup.alpha = 1;
            exitGroup.interactable = true;

            if (activeDialogBox.hasPorts)
            {
                // initialize button selection if using gamepad
                dialogBox.SelectFirstPortButton();
            }
        }

        // clicked on one of the dialog branch options
        void SelectPort(string portName)
        {
            StartCoroutine(DelayAndSelectPort(portName));
        }
        // delay one frame to input from registering
        IEnumerator DelayAndSelectPort(string portName)
        {
            yield return null;
            if (activeDialogBox == null || !activeDialogBox.finishedReading) yield break;
            var box = activeDialogBox;
            activeDialogBox = null;
            StopCoroutine(box.coroutine);
            dialogBox.Close();
            box.finishEvent(false, portName);
        }

        // clicked the submit button, continues when there are no branches
        private void UISubmit(InputAction.CallbackContext obj)
        {
            if (activeDialogBox == null || !activeDialogBox.finishedReading) return;
            if (activeDialogBox.hasPorts) return; // ports need explicit selection for branching

            SelectPort("DEFAULT");
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
            }
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
        //readonly Regex RegexWhitespace = new Regex(@"/\S+\s\S+/");
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
    }
}