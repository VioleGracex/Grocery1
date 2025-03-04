using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Truck : MonoBehaviour
{
    public static Truck Instance;
    private Dictionary<string, int> items = new Dictionary<string, int>();
    [SerializeField]
    private TextMeshProUGUI itemListText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        ItemGrabbable item = other.GetComponent<ItemGrabbable>();
        if (item != null)
        {
            AddItem(item);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ItemGrabbable item = other.GetComponent<ItemGrabbable>();
        if (item != null)
        {
            RemoveItem(item);
        }
    }

    public void AddItem(ItemGrabbable item)
    {
        if (items.ContainsKey(item.itemName))
        {
            items[item.itemName]++;
        }
        else
        {
            items[item.itemName] = 1;
        }
        UpdateUI();
    }

    public void RemoveItem(ItemGrabbable item)
    {
        if (items.ContainsKey(item.itemName))
        {
            items[item.itemName]--;
            if (items[item.itemName] <= 0)
            {
                items.Remove(item.itemName);
            }
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        string itemList = "";
        foreach (var item in items)
        {
            itemList += item.Key + " " + item.Value + "<br>";
        }

        itemListText.text = itemList;
    }
}