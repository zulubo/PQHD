using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PQHD.Dialog
{
    public class DialogEventRelay : MonoBehaviour
    {
        [System.Serializable]
        public class DialogEvent
        {
            public string key;
            public UnityEngine.Events.UnityEvent OnTrigger;
        }
        public List<DialogEvent> events = new List<DialogEvent>();

        public void TriggerEvent(string key)
        {
            if (events == null) return;
            var e = events.Find(ev => string.Equals(ev.key, key, System.StringComparison.OrdinalIgnoreCase));
            if (e != null) e.OnTrigger.Invoke();
            else Debug.LogWarning("event not found: " + key, this);
        }
    }
}