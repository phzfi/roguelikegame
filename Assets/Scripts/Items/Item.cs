﻿using UnityEngine;
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
    public int m_strength = 0;
    [SyncVar]
    public int m_agility = 0;
    [SyncVar]
    public int m_intelligence = 0;
    [SyncVar]
    public int m_vitality = 0;
	public bool m_twoHandedWeapon = false;

    [SyncVar]
	private bool m_onMap = true;

    public enum ItemType { WEAPON, HEAD, BODY, LEGS, RING, SHIELD, OTHER };

    public ItemType m_typeOfItem = ItemType.OTHER;

    void Start()
	{
		ItemManager.Register(this, m_onMap);
		float z = transform.position.z;
		Vector3 vec = MapGrid.GridToWorldPoint(m_pos);
		vec.z = z;
		transform.position = vec;

		if (!m_onMap) // if item has been picked up already
		{
			gameObject.SetActive(false);
		}
	}

	public void Pickup(GameObject obj) // Adds this item to given object's inventory, if it has room
	{
		var inventory = obj.GetComponent<Inventory>();
		if (inventory == null || !inventory.AddItem(gameObject))
			return;
		ItemManager.UnregisterFromMap(ID);
		m_onMap = false;
	}

	public bool CanPickup(GameObject obj) // Finds out if this item will fit into given object's inventory
	{
		var inventory = obj.GetComponent<Inventory>();
		if (inventory == null || !inventory.CanAddItem(gameObject))
			return false;
		return true;
	}

	public void Hide()
	{
		for (int i = 0; i < transform.childCount; ++i)
			transform.GetChild(i).gameObject.SetActive(false);
	}

	public void Show()
	{
		for (int i = 0; i < transform.childCount; ++i)
			transform.GetChild(i).gameObject.SetActive(true);
	}

	void OnDestroy()
	{
		ItemManager.Unregister(ID);
	}

    public override string ToString()
    {
        string information = "";
		if (m_typeOfItem == ItemType.OTHER)
		{
			var hp = GetComponent<HealthPack>();
			if (hp != null)
				information = "Heals " + hp.m_heals + " hitpoints.";
			else
				information = m_name;
		}
		else
		{
			information = "Strength: " + m_strength
						+ "\nAgility: " + m_agility
						+ "\nIntelligence: " + m_intelligence
						+ "\nVitality: " + m_vitality;
		}

        return information;
    }
}
