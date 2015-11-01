using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Inventory : MonoBehaviour {

    public int m_maxItems = 5;
    private List<GameObject> m_items;

	// Use this for initialization
	void Start ()
    {
        m_items = new List<GameObject>();
	}

    public bool AddItem(GameObject item)
    {
        if (m_items.Count < m_maxItems)
        {
            m_items.Add(item);
            Debug.Log("Picked up item: " + item.GetComponent<Item>().m_name);
            return true;
        }
        return false;
    }
}
