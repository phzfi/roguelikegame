using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
	public AudioClip m_itemPickupAudio;
	public AudioClip m_coinPickupAudio;
	public int m_maxItems = 5;
	public static int sm_amountOfCoins = 0;
    public GameObject m_inventoryPanel;
    public GameObject m_itemUIComponent;
    

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
		if (CanAddItem(item) && itemName != "Coins")
		{
			m_items.Add(item);
			m_audioSource.PlayOneShot(m_itemPickupAudio);
			Debug.Log("Picked up item: " + itemName + ", ID: " + item.GetComponent<Item>().ID);
            //Image image = item.GetComponent<Image>();
            //Debug.Log("image " + image);
            //var a = Instantiate(m_itemUIComponent)as GameObject;
            //Debug.Log("a " + a);
            //Debug.Log(a.GetComponent<Image>().sprite + " sprite " + image.sprite);
            //Debug.Log("a image " + a.GetComponent<Image>());
            //var imageSprite = a.GetComponent<Image>().sprite;
            //imageSprite = image.sprite;
            //var parent = GameObject.FindGameObjectWithTag("Inventory");
            //a.transform.SetParent(parent.transform);
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
