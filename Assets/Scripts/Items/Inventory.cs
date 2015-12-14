﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour, IDropHandler
{
	public AudioClip m_itemPickupAudio;
	public AudioClip m_coinPickupAudio;
	public int m_maxItems = 5;
	public static int sm_amountOfCoins = 0;
    public GameObject m_UIElements;

    private List<GameObject> m_items;
	private AudioSource m_audioSource;
    private GameObject m_inventoryCanvas;
    

	void Start()
	{
		m_items = new List<GameObject>();
		m_audioSource = GetComponent<AudioSource>();
        m_inventoryCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas");
        var playerElements = Instantiate(m_UIElements);
        playerElements.transform.parent = m_inventoryCanvas.transform;
        playerElements.name = "UI " + GetComponent<SimpleCharacterMovement>().ID;
	}

    public void OnDrop(PointerEventData eventData)
    {

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
            AddToUIInventory(item);
			return true;
		}
		if (itemName == "Coins")
		{
			Inventory.sm_amountOfCoins += 1;
			Debug.Log(Inventory.sm_amountOfCoins);
			m_audioSource.PlayOneShot(m_coinPickupAudio);
			return true;
		}
		return false;
	}

    //Moves the item to be a child of InventoryPanel UI component. The item's Image component's sprite is 
    //then rendered to the panel.
    public void AddToUIInventory(GameObject item)
    {
        var parent = GameObject.FindGameObjectWithTag("InventoryCanvas");
        for(int i = 0; i < parent.transform.childCount; ++i)
        {
            var child = parent.transform.GetChild(i);
            if (child.tag == "Inventory")
            {
                item.transform.SetParent(child.GetChild(0));
                break;
            }
        }        
    }
}
