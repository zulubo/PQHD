using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace PQHD
{
    public class SettingsOptionFillBar : MonoBehaviour
    {
        [SerializeField] string id;
        [SerializeField] Graphic[] bar;
        [SerializeField] Color color_on = Color.white;
        [SerializeField] Color color_off = Color.navyBlue;
        [SerializeField] SimpleUISelectable selectable;
        private SettingsMenu menu;

        [SerializeField] UnityEngine.Audio.AudioResource changeSound;

        private int value;

        void OnEnable()
        {
            if(menu == null) menu = GetComponentInParent<SettingsMenu>();
            value = menu.GetSetting(id);
            ClampValue();
            UpdateOptions();
        }

        void ClampValue()
        {
            if(value < 0) value = 0;
            if(value > bar.Length) value = bar.Length;
        }

        void UpdateOptions()
        {
            for(int i = 0; i < bar.Length; i++)
            {
                bar[i].color = value > i ? color_on : color_off;
            }
        }

        void Update()
        {
            if(selectable.hovering)
            {
                if(Input.MoveAxis.DPadLeft.WasPressedThisFrame) Change(-1);
                if(Input.MoveAxis.DPadRight.WasPressedThisFrame) Change(1);    
            }
        }

        private void Change(int dir)
        {
            int oldVal = value;
            value += dir;
            ClampValue();
            UpdateOptions();
            menu.SetSetting(id, value);
            if(value != oldVal)
            {
                Audio.I.PlaySound2D(changeSound);
            }
        }
    }
}
