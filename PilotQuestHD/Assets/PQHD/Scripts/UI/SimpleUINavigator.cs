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
        [SerializeField] RectTransform pointer;
        [SerializeField] private bool selectFlicker = false;
        [SerializeField] AudioResource selectSound;
        [SerializeField] AudioResource hoverSound;

        public int HoverIndex { get; private set; }
        public SimpleUISelectable Hovering
        {
            get
            {
                if(HoverIndex == -1) return null;
                if(selectables.Count == 0) return null;
                return selectables[HoverIndex];
            }
        }

        private List<SimpleUISelectable> selectables = new();

        public void Register(SimpleUISelectable sel)
        {
            selectables.Add(sel);
            SortSelectables();

            if(!Hovering)
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
            // sort by height
            selectables.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
        }

        public void Hover(int index, bool playSound = true)
        {
            int oldHover = HoverIndex;
            HoverIndex = index;
            ValidateHovering();
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

        private void ValidateHovering()
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

            if(selectFlicker) StartCoroutine(SelectFlickerCoroutine(Hovering));
            else Hovering.onSelect.Invoke();
        }

        private bool selecting = false;
        IEnumerator SelectFlickerCoroutine(SimpleUISelectable sel)
        {
            selecting = true;
            float duration = 1f;
            float period = 0.05f;
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
            selecting = false;
        }

        private void Update()
        {
            if(selecting) return;

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
