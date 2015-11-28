using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
	public AudioClip m_itemPickupAudio;
	public AudioClip m_coinPickupAudio;
	public int m_maxItems = 5;
	public static int sm_amountOfCoins = 0;

	private List<GameObject> m_items;
	private AudioSource m_audioSource;

	// Use this for initialization
	void Start()
	{
		m_items = new List<GameObject>();
		m_audioSource = GetComponent<AudioSource>();
	}

	public bool CanAddItem(GameObject item)
	{
		return m_items.Count < m_maxItems;
	}

	public bool AddItem(GameObject item)
	{
		var itemName = item.GetComponent<Item>().m_name;
		if (m_items.Count < m_maxItems && itemName != "Coins")
		{
			m_items.Add(item);
			m_audioSource.PlayOneShot(m_itemPickupAudio);
			Debug.Log("Picked up item: " + itemName + ", ID: " + item.GetComponent<Item>().ID);
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
}
