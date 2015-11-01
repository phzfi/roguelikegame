using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {

    public string m_name = "Palikka";

    private bool m_onMap = true;

	// Use this for initialization
	void Start () {
        ItemManager.sm_allItems.Add(gameObject);
        ItemManager.sm_itemsOnMap.Add(gameObject);
    }
	
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        var obj = other.gameObject;
        if (obj.tag != "Player")
            return;

        var inventory = obj.GetComponent<Inventory>();
        if (inventory == null || !inventory.AddItem(gameObject))
            return;

        gameObject.SetActive(false);
        ItemManager.sm_itemsOnMap.Remove(gameObject);
        m_onMap = false;
    }

    void OnDestroy()
    {
        ItemManager.sm_allItems.Remove(gameObject);
        if(m_onMap)
            ItemManager.sm_itemsOnMap.Remove(gameObject);
    }
}
