using UnityEngine;
using TMPro;

namespace PQHD
{
    public class SettingsOptionMulti : MonoBehaviour
    {
        [SerializeField] string id;
        [SerializeField] TMP_Text optionText;
        [SerializeField] string[] options;
        [SerializeField] SimpleUISelectable selectable;
        [SerializeField] bool loopValue = true;
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
            if(loopValue)
            {
                value = IntExtensions.Modulo(value, options.Length);
            }
            else
            {
                if(value < 0) value = 0;
                if(value >= options.Length) value = options.Length - 1;
            }
        }

        void UpdateOptions()
        {
            //for(int i = 0; i < options.Length; i++)
            //{
            //    options[i].SetActive(i == value);
            //}
            optionText.text = options[value];
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
