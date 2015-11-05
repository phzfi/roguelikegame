using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {

    public string m_name = "Palikka"; // placeholder
    public Vector2i m_pos;
    public int ID;

    private bool m_onMap = true;
    
	void Start () {
        ItemManager.Register(this, out ID);
        m_pos = MovementManager.sm_grid.GetGridPosition(transform.position);
    }
	
	void Update () {
	
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
