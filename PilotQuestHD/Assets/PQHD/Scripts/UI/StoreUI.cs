using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands.FastExport;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PQHD
{
    public class StoreUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text headerText;
        [System.Serializable]
        private class UIArray
        {
            public GameObject prefab;
            public Transform root;
            public GameObject Create()
            {
                GameObject inst = Instantiate(prefab, root);
                inst.SetActive(true);
                instances.Add(inst);
                return inst;
            }

            [NonSerialized]
            public List<GameObject> instances = new();
            public void Clear()
            {
                for(int i = 0; i < instances.Count; i++)
                {
                    if(instances[i]) Destroy(instances[i]);
                }
                instances.Clear();
            }
        }
        [SerializeField] UIArray currenciesDisplay;
        [SerializeField] UIArray itemsDisplay;

        [SerializeField] SimpleUINavigator itemNav;

        bool IsActive => StoreRuntime.active;
        StoreRuntime ActiveStore => StoreRuntime.active;
        StoreDef ActiveStoreDef => StoreRuntime.active.def;

        void Start()
        {
            StoreRuntime.onChangeActive += () => UpdateUI();
        }

        public void UpdateUI(bool keepSelection = false)
        {
            int selection = itemNav.HoverIndex;
            
            if(IsActive)
            {
                itemsDisplay.Clear();
                currenciesDisplay.Clear();
                
                headerText.text = ActiveStoreDef.header;

                for(int i = 0; i < ActiveStoreDef.items.Length; i++)
                {
                    StoreDef.Item item = ActiveStoreDef.items[i];
                    GameObject itemUI = itemsDisplay.Create();
                    itemUI.GetComponentInChildren<TMP_Text>().text = item.loot.displayName;
                    SimpleUISelectable sel = itemUI.GetComponent<SimpleUISelectable>();
                    sel.onSelect.AddListener(() => Buy(item));
                    sel.Active = ActiveStore.inventory[item] > 0;
                    sel.UpdateColor();
                }

                if(keepSelection) itemNav.Hover(selection);

                for(int c = 0; c < ActiveStoreDef.currencies.Length; c++)
                {
                    GameObject currencyUI = currenciesDisplay.Create();
                    currencyUI.transform.Find("icon/icon").GetComponent<Image>().sprite = ActiveStoreDef.currencies[c].icon;
                    UIArray prices = new UIArray()
                    {
                        prefab = currencyUI.transform.Find("prices/price text").gameObject,
                        root = currencyUI.transform.Find("prices"),
                    };

                    for(int i = 0; i < ActiveStoreDef.items.Length; i++)
                    {
                        GameObject priceUI = prices.Create();
                        int? price = ActiveStoreDef.items[i].GetCost(ActiveStoreDef.currencies[c]);
                        TMP_Text text = priceUI.GetComponent<TMP_Text>();
                        text.enabled = price.HasValue;
                        if(price.HasValue) text.text = price.ToString();
                    }
                }

                root.SetActive(true);
            }
            else
            {
                root.SetActive(false);
            }
        }

        void Update()
        {
            if(IsActive)
            {
                if(Input.ButtonB.WasPressedThisFrame)
                {
                    ActiveStore.Close();
                }
            }
        }

        void Buy(StoreDef.Item item)
        {
            if(!IsActive) return;

            if(item.CanAfford())
            {
                ActiveStore.Buy(item);
                UpdateUI();
            }
            else
            {
                // show error text?
            }

        }
    }
}
