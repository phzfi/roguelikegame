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
    public EquipUI m_equipmentSlotsParent;
    public InventorySlots m_inventorySlotsParent;

	private AudioSource m_audioSource;
    private Slot[] m_inventorySlots;
    private Slot[] m_equipmentSlots;

	void Start()
	{
		m_items = new List<GameObject>();
		m_audioSource = GetComponent<AudioSource>();
        m_equipmentSlotsParent = Resources.FindObjectsOfTypeAll<EquipUI>()[0];
        if (m_equipmentSlotsParent == null)
            Debug.Log("equipment slots not found in inventory");
        m_inventorySlotsParent = Resources.FindObjectsOfTypeAll<InventorySlots>()[0];
        if (m_inventorySlotsParent == null)
            Debug.Log("inventory slots not found in inventory");
        Debug.Log(m_inventorySlotsParent.gameObject);
        m_inventorySlots = m_inventorySlotsParent.gameObject.GetComponentsInChildren<Slot>(true);
        m_equipmentSlots = m_equipmentSlotsParent.gameObject.GetComponentsInChildren<Slot>(true);
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
        int amountOfPotions = AmountOfItem("Potion");
        for(int i = 0; i < m_inventorySlots.Length; ++i)
        {
            var child = m_inventorySlots[i].transform.GetChild(0);
            var item = child.GetComponent<Item>();
            if(item != null)
            {
                if(item.m_name == "Potion")
                {
                    var amountText = item.GetComponentsInChildren<Text>(true)[0];
                    amountText.text = amountOfPotions.ToString();
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

    //Moves the item to be a child of InventoryPanel UI component. The item's Image component's sprite is 
    //then rendered to the panel.
    public void AddToUIInventory(GameObject item)
    {
        for(int i = 0; i < m_inventorySlots.Length; ++i)
        {
            var slot = m_inventorySlots[i];
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
