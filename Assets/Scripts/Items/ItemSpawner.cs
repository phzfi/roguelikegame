using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

// Class that spawns a single instance of item prefab determined by editor, then self destructs. Handles spawn syncing.
public class ItemSpawner : NetworkBehaviour
{
	public GameObject m_item;

	public void Start()
	{
		GameObject obj = (GameObject)Instantiate(m_item, transform.position, transform.rotation);
		var item = obj.GetComponent<Item>();

		ItemManager.GetID(out item.ID);
		//item.m_name = "Palikka" + item.ID;
		item.m_pos = MapGrid.WorldToGridPoint(transform.position);

		NetworkServer.Spawn(obj);
		Destroy(gameObject);
	}
}
