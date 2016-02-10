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
    private CharController m_player;

	void Start()
	{
		m_items = new List<GameObject>();
		m_audioSource = GetComponent<AudioSource>();
        m_player = GetComponent<CharController>();
	}

    public int AmountOfItem(string itemName)
    {
        int count = 0;
        for(int i = 0; i < m_items.Count; ++i)
        {
            var item = m_items[i].GetComponent<Item>();
            if (item.m_name == itemName)
                count += 1;
        }
        return count;
    }

	public void UpdatePotionCount()
	{
		var slots = InventorySlots();
		int amountOfPotions = AmountOfItem("Potion");
		for (int i = 0; i < slots.transform.childCount; ++i)
		{
			var child = slots.transform.GetChild(i);
			if (child.childCount == 0)
				continue;
			var item = child.GetChild(0).gameObject.GetComponent<Item>();
			if (item != null)
			{
				if (item.m_name == "Potion")
				{
					var text = item.GetComponentsInChildren<Text>(true)[0];
					text.text = amountOfPotions.ToString();
					return;
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
            if (!m_player.isLocalPlayer)
            {
                item.SetActive(false);
                return true;
            }

            if (itemName == "Potion")
            {
                if (AmountOfItem("Potion") <= 1)
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

    public GameObject InventorySlots()
    {
        var inventoryCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas").transform.GetChild(0);
        for (int i = 0; i < inventoryCanvas.childCount; ++i)
        {
            var child = inventoryCanvas.GetChild(i);
            if (child.tag == "Inventory")
            {
                for(int j = 0; j < child.GetChild(0).childCount; ++j)
                {
                    var content = child.GetChild(0).GetChild(j);
                    if(content.tag == "InventorySlots")
                    {
                        return content.gameObject;
                    }
                }
            }  
        }
        return null;
    }

    //Moves the item to be a child of InventoryPanel UI component. The item's Image component's sprite is 
    //then rendered to the panel.
    public void AddToUIInventory(GameObject item)
    {
        GameObject inventorySlots = InventorySlots();

        for(int i = 0; i < inventorySlots.transform.childCount; ++i)
        {
            var slot = inventorySlots.transform.GetChild(i).GetComponent<Slot>();
            if(!slot.m_containsItem)
            {
                item.transform.SetParent(slot.transform);
                item.GetComponent<RectTransform>().localScale = Vector3.one;
                slot.m_containsItem = true;
                break;
            }
        }    
    }

    
}
