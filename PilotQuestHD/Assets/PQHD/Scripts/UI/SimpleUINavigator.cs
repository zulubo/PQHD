using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace PQHD
{
    /// <summary>
    /// Simple navigator for vertical lists of selectables
    /// </summary>
    public class SimpleUINavigator : MonoBehaviour
    {
        public static List<SimpleUINavigator> all = new();
        
        [Tooltip("Priority for choosing active navigator when multiple are active. Make sure each has a different priority to prevent conflicts")]
        [SerializeField] int priority;
         
        [SerializeField] RectTransform pointer;
        [SerializeField] Animator pointerAnim;
        [SerializeField] private bool selectFlicker = false;
        [SerializeField] AudioResource selectSound;
        [SerializeField] AudioResource hoverSound;

        public int HoverIndex { get; private set; } = -1;
        public SimpleUISelectable Hovering
        {
            get
            {
                if(HoverIndex == -1) return null;
                if(HoverIndex >= selectables.Count) return null;
                if(selectables.Count == 0) return null;
                return selectables[HoverIndex];
            }
        }

        private List<SimpleUISelectable> selectables = new();

        void Start()
        {
            if(pointerAnim) pointerAnim.keepAnimatorStateOnDisable = true;
        }

        void OnEnable()
        {
            all.Add(this);
        }

        void OnDisable()
        {
            all.Remove(this);
            HoverIndex = -1;
            UpdateVisuals();
            UpdatePointer();
        }

        public void Register(SimpleUISelectable sel)
        {
            selectables.Add(sel);
            SortSelectables();

            if(sel.hoverByDefault && sel.Active)
            {
                Hover(selectables.IndexOf(sel));
            }
            else if(!Hovering)
            {
                Hover(0);
            }
        }

        public void DeRegister(SimpleUISelectable sel)
        {
            selectables.Remove(sel);
        }

        public void SortSelectables()
        {
            SimpleUISelectable oldHovering = Hovering;
            
            selectables.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            if(oldHovering)
            {
                Hover(selectables.IndexOf(oldHovering));
            }
        }

        public void Hover(int index, bool playSound = true)
        {
            int oldHover = HoverIndex;
            HoverIndex = index;
            ValidateHoverIndex();
            if(HoverIndex != oldHover)
            {
                if(playSound && hoverSound) Audio.I.PlaySound2D(hoverSound);
            }
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            for(int s = 0; s < selectables.Count; s++)
            {
                selectables[s].hovering = s == HoverIndex;
                selectables[s].UpdateColor();
            }
        }

        private void ValidateHoverIndex()
        {
            if(selectables.Count == 0)
            {
                HoverIndex = -1;
                return;
            } 

            if(HoverIndex < 0) HoverIndex = 0;
            if(HoverIndex >= selectables.Count) HoverIndex = selectables.Count - 1;

            if(!Hovering.Active)
            {
                for(int s = 0; s < selectables.Count; s++)
                {
                    if(selectables[s].Active)
                    { 
                        HoverIndex = s;
                        return;
                    }
                }
                HoverIndex = -1;
            }
        }

        private void Select()
        {
            if(Hovering == null) return;

            if(selectSound) Audio.I.PlaySound2D(selectSound);
            if(Hovering.validateSelect != null && !Hovering.validateSelect()) return;
            if(selectFlicker) StartCoroutine(SelectFlickerCoroutine(Hovering));
            else Hovering.onSelect.Invoke();
        }

        private bool selecting = false;
        IEnumerator SelectFlickerCoroutine(SimpleUISelectable sel)
        {
            selecting = true;
            
            for(int s = 0; s < selectables.Count; s++)
            {
                selectables[s].Flicker(false);    
            }

            if(pointerAnim) pointerAnim.speed = 0;
            float duration = 1f;
            float period = 0.03f;
            float t = 0;
            bool on = false;
            while(t < duration)
            {
                sel.Flicker(on);
                pointer.gameObject.SetActive(on);
                on = !on;
                t += period;
                yield return new WaitForSeconds(period);
            }
            sel.onSelect.Invoke();
            if(pointerAnim) pointerAnim.speed = 1;
            for(int s = 0; s < selectables.Count; s++)
            {
                selectables[s].Flicker(true);    
            }
            selecting = false;
        }

        private void Update()
        {
            if(selecting || SceneSwitcher.Transitioning) return;

            bool overridden = false;
            // make sure we are the primary navigator
            for(int n = 0; n < all.Count; n++)
            {
                if(all[n] == this) continue;
                if(all[n].priority == priority) Debug.LogError("Multiple SimpleUINavigators have same priority");
                if(all[n].priority > priority)
                {
                    overridden = true;
                    break;
                }
            }

            if(overridden)
            {
                pointer.gameObject.SetActive(false);
                return;
            }

            UpdatePointer();
            UpdateNav();
        }

        void UpdatePointer()
        {
            pointer.gameObject.SetActive(Hovering);
            if(Hovering)
            {
                RectTransform selRect = Hovering.transform as RectTransform;
                // place pointer at left center of selectable
                Vector3 pointerPoint = selRect.TransformPoint(new Vector3(selRect.rect.xMin, selRect.rect.center.y, 0));
                pointer.position = pointerPoint;
            }
        }

        void UpdateNav()
        {
            if(!Hovering || !Hovering.Active)
            {
                ValidateHoverIndex();
                if(HoverIndex != -1) Hover(HoverIndex);
            }

            if(Input.MoveAxis.DPadUp.WasPressedThisFrame)
            {
                Hover(HoverIndex - 1);
            }
            if(Input.MoveAxis.DPadDown.WasPressedThisFrame)
            {
                Hover(HoverIndex + 1);
            }

            if(Hovering && Input.ButtonA.WasPressedThisFrame || Input.ButtonB.WasPressedThisFrame)
            {
                Select();
            }
        }
    }
}
