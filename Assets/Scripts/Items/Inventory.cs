using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Inventory : MonoBehaviour
{
	public AudioClip m_itemPickupAudio;
	public AudioClip m_coinPickupAudio;
	public int m_maxItems = 5;
	public int m_amountOfCoins = 0;
    public List<GameObject> m_items;

	private AudioSource m_audioSource;    

	void Start()
	{
		m_items = new List<GameObject>();
		m_audioSource = GetComponent<AudioSource>();
	}

    public int AmountOfPotions()
    {
        int count = 0;
        for(int i = 0; i < m_items.Count; ++i)
        {
            var item = m_items[i].GetComponent<Item>();
            if (item.m_name == "Potion")
                count += 1;
        }
        return count;
    }

    public void UpdatePotionCount()
    {
        var inventoryPanel = InventoryPanel();
        int amountOfPotions = AmountOfPotions();
        for(int i = 0; i < inventoryPanel.transform.childCount; ++i)
        {
            var child = inventoryPanel.transform.GetChild(i);
            var item = child.gameObject.GetComponent<Item>();
            if(item != null)
            {
                if(item.m_name == "Potion")
                {
                    Debug.Log(item.gameObject);
                    for(int j = 0; i < item.transform.childCount; ++j)
                    {
                        var text = item.transform.GetChild(j).GetComponent<Text>();
                        if (text == null)
                        {
                            continue;
                        }
                        text.text = amountOfPotions.ToString();
                        break;
                    }
                    break;
                }
            }
        }
    }

    public bool CanAddItem(GameObject item)
	{
		return m_items.Count < m_maxItems;
	}

	public bool AddItem(GameObject item)
	{
		var itemName = item.GetComponent<Item>().m_name;
		if (CanAddItem(item) && itemName != "Coins")
		{
            m_items.Add(item);
            m_audioSource.PlayOneShot(m_itemPickupAudio);
			Debug.Log("Picked up item: " + itemName + ", ID: " + item.GetComponent<Item>().ID);
            if (itemName == "Potion")
            {
                if (AmountOfPotions() <= 1)
                    AddToUIInventory(item);
                else
                {
                    UpdatePotionCount();
                    item.SetActive(false);
                }
                    
            }
            else
                AddToUIInventory(item);
            
            return true;
		}
		if (itemName == "Coins")
		{
            item.SetActive(false);
			m_amountOfCoins += 1;
			Debug.Log(m_amountOfCoins);
			m_audioSource.PlayOneShot(m_coinPickupAudio);
			return true;
		}
		return false;
	}

    public GameObject InventoryPanel()
    {
        var inventoryCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas");
        for (int i = 0; i < inventoryCanvas.transform.childCount; ++i)
        {
            var child = inventoryCanvas.transform.GetChild(i);
            if (child.tag == "Inventory")
                return child.GetChild(0).gameObject;
        }
        return null;
    }

    //Moves the item to be a child of InventoryPanel UI component. The item's Image component's sprite is 
    //then rendered to the panel.
    public void AddToUIInventory(GameObject item)
    {
        var inventoryPanel = InventoryPanel();
        item.transform.SetParent(inventoryPanel.transform);      
    }
}
