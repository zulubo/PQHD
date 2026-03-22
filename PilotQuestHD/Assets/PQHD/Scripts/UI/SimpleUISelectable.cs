using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PQHD
{
    public class SimpleUISelectable : MonoBehaviour
    {
        SimpleUINavigator navigator;

        public Func<bool> validateSelect;
        public UnityEvent onSelect;

        [SerializeField] Graphic graphic;

        public bool hoverByDefault;

        public bool canSelect = true;


        [SerializeField] Color defaultColor = Color.white;
        [SerializeField] Color hoverColor = Color.yellow;
        [SerializeField] Color inactiveColor = Color.gray3;
        [SerializeField] GameObject hoverObject;

        [SerializeField] private bool _active = true;
        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                UpdateVisual();
            }
        }

        [NonSerialized] public bool hovering;

        void OnEnable()
        {
            navigator = GetComponentInParent<SimpleUINavigator>();
            if(navigator) navigator.Register(this);
        }

        void OnDisable()
        {
            if(navigator) navigator.DeRegister(this);
        }

        public void UpdateVisual()
        {
            if(graphic != null)
            {
                graphic.color = defaultColor;
                if(hovering) graphic.color = hoverColor;
                if(!Active) graphic.color = inactiveColor;
            }
            if(hoverObject) hoverObject.SetActive(hovering);
        }

        public void Flicker(bool on)
        {
            graphic.enabled = on;
        }
    }
}
