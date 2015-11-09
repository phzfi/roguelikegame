using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Item : NetworkBehaviour
{

	[SyncVar]
	public string m_name = "Palikka"; // placeholder
	[SyncVar]
	public Vector2i m_pos;
	[SyncVar]
    public int ID = -1;

	[SyncVar]
	private bool m_onMap = true;

    void Start()
    {
		ItemManager.Register(this, m_onMap);
		transform.position = MovementManager.sm_grid.GetWorldPos(m_pos);

		if(!m_onMap) // if item has been picked up already
		{
			gameObject.SetActive(false);
		}
	}

    void Update()
    {

    }

    public void Pickup(GameObject obj)
    {
        var inventory = obj.GetComponent<Inventory>();
        if (inventory == null || !inventory.AddItem(gameObject))
            return;

        gameObject.SetActive(false);
        ItemManager.UnregisterFromMap(ID);
        m_onMap = false;
    }

    public bool CanPickup(GameObject obj)
    {
        var inventory = obj.GetComponent<Inventory>();
        if (inventory == null || !inventory.CanAddItem(gameObject))
            return false;
        return true;
    }

    void OnDestroy()
    {
        ItemManager.Unregister(ID);
    }
}
