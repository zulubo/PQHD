using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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
        
        [SerializeField] AudioResource buySound;
        [SerializeField] AudioResource errorSound;

        bool IsActive => StoreRuntime.active;
        StoreRuntime ActiveStore => StoreRuntime.active;
        StoreDef ActiveStoreDef => StoreRuntime.active.def;

        void Start()
        {
            StoreRuntime.onChangeActive += ActiveStoreChanged;
        }

        void OnDestroy()
        {
            StoreRuntime.onChangeActive -= ActiveStoreChanged;
        }

        void ActiveStoreChanged()
        {
            UpdateUI();
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
                    bool bought = ActiveStore.inventory[item] <= 0;
                    TMP_Text text = itemUI.GetComponentInChildren<TMP_Text>();
                    text.text = bought ? "- Bought -" : item.loot.displayName;
                    text.color = bought ? Color.gray2 : Color.white; 
                    SimpleUISelectable sel = itemUI.GetComponent<SimpleUISelectable>();
                    sel.onSelect.AddListener(() => Buy(item));
                }

                if(keepSelection) itemNav.Hover(selection, false);

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
                        StoreDef.Item item = ActiveStoreDef.items[i];
                        GameObject priceUI = prices.Create();
                        bool bought = ActiveStore.inventory[item] <= 0;
                        int? price = null;
                        if(!bought) price = ActiveStoreDef.items[i].GetCost(ActiveStoreDef.currencies[c]);
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

            if(ActiveStore.inventory[item] > 0)
            {
                if(item.CanAfford())
                {
                    ActiveStore.Buy(item);
                    UpdateUI();
                    Audio.I.PlaySound2D(buySound);
                    if(!string.IsNullOrEmpty(item.buyText))
                    {
                        headerText.text = item.buyText;
                    }
                }
                else
                {
                    StartCoroutine(ErrorCoroutine("Sorry, you don't have enough resources!"));
                }
            }
            else
            {
                StartCoroutine(ErrorCoroutine("You already bought this! (And I'm very grateful for it!)"));
            }
        }

        IEnumerator ErrorCoroutine(string errorText)
        {
            int hoverIndex = itemNav.HoverIndex;
            itemNav.enabled = false;
            headerText.text = errorText;
            Audio.I.PlaySound2D(errorSound);
            yield return new WaitForSeconds(0.6f);
            itemNav.enabled = true;
            itemNav.Hover(hoverIndex, false);
        }
    }
}
