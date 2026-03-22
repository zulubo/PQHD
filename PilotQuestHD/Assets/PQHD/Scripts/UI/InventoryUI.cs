using System;
using System.Collections.Generic;
using PQHD.Dialog;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace PQHD
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        private Vector2 defaultPos;
        [SerializeField] private Vector2 offsetDuringDialog;
        private bool isDialogOffset;
        [SerializeField] private GameObject lootPrefab;

        [SerializeField] bool hideWhenStoreOpen;

        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color atCapacityColor = new Color(1, 0.2f, 0.2f);

        private class LootDisplay
        {
            public Loot loot;
            public GameObject gameObject;
            public Image icon;
            public TMP_Text numText;
            public Image useIcon;

            private SpringDamp bounceSim;
            private float bounceTimer;

            private int _count;

            private InventoryUI ui;
            
            public LootDisplay(GameObject prefab, Loot loot, InventoryUI ui)
            {
                this.loot = loot;
                this.ui = ui;
                gameObject = Instantiate(prefab, prefab.transform.parent);
                icon = gameObject.transform.Find("icon").GetComponent<Image>();
                numText = gameObject.transform.Find("numText").GetComponent<TMP_Text>();
                useIcon = gameObject.transform.Find("useIcon").GetComponent<Image>();

                icon.sprite = loot.icon;
                numText.gameObject.SetActive(!loot.unique && !loot.useIcon);
                useIcon.gameObject.SetActive(loot.useIcon);
                if (loot.useIcon) useIcon.sprite = loot.useIcon;

                bounceSim = new SpringDamp(1, BounceSpring, BounceDamp);
            }


            private const float BounceBump = 16;
            private const float BounceSpring = 1000;
            private const float BounceDamp = 22;
            private const float BounceDuration = 1f;
            public void SetCount(int count)
            {
                if(loot.hideInInventoryUI)
                {
                    gameObject.SetActive(false);
                    return;
                }

                if (_count != count)
                {
                    gameObject.SetActive(count > 0);
                    numText.text = count.ToString();
                    
                    bounceSim.Bump(BounceBump);
                    bounceTimer = BounceDuration;

                    _count = count;
                }

                numText.color = _count >= Inventory.I.GetCapacity(loot) ? ui.atCapacityColor : ui.defaultColor;
            }

            public void Update()
            {
                if (bounceTimer > 0)
                {
                    float dt = Mathf.Min(Time.deltaTime, 0.01666666666f);
                    bounceTimer -= dt;
                    bounceSim.UpdateSubstepped(1, dt, 2);
                    numText.transform.localScale = new Vector3(1, bounceSim.Position, 1);
                }
            }
        }

        private LootDisplay[] displays;
        
        private Loot[] loot => Inventory.I.database.loot;

        void Awake()
        {
            defaultPos = rect.anchoredPosition;
        }

        private void OnEnable()
        {
            UpdateUI();
            Inventory.onChanged += UpdateUI;
            StoreRuntime.onChangeActive += UpdateUI;
        }

        void OnDisable()
        {
            Inventory.onChanged -= UpdateUI;
            StoreRuntime.onChangeActive -= UpdateUI;
        }

        private bool initUI;

        void UpdateUI()
        {
            if (!Inventory.I) return;

            if(hideWhenStoreOpen && StoreRuntime.active)
            {
                rect.gameObject.SetActive(false);
                return;
            }
                
            rect.gameObject.SetActive(true);
            
            if (displays == null)
            {
                displays = new LootDisplay[loot.Length];
                for (int i = 0; i < loot.Length; i++)
                {
                    displays[i] = new LootDisplay(lootPrefab, loot[i], this);
                }
            }
            
            for (int i = 0; i < loot.Length; i++)
            {
                displays[i].SetCount(Inventory.I.contents[loot[i]]);
            }

            initUI = true;
        }

        void Update()
        {
            if(!Inventory.I || !initUI) return;
            
            for (int i = 0; i < loot.Length; i++)
            {
                displays[i].Update();
            }

            bool shouldDialogOffset = DialogManager.IsPlayingDialog;

            if (shouldDialogOffset != isDialogOffset)
            {
                isDialogOffset = shouldDialogOffset;
                rect.anchoredPosition = isDialogOffset ? defaultPos + offsetDuringDialog : defaultPos;
            }
        }
    }
}
