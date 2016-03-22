using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Slot : MonoBehaviour, IDropHandler
{
    public bool m_isInventory = false;
    public Item.ItemType m_itemType = Item.ItemType.OTHER;
    public bool m_containsItem = false;
    public AudioClip m_equipSound;

    private Equipment m_equipment;
	private Inventory m_inventory;
    private int m_playerID;
    private AudioSource m_audioSource;
	private GameObject m_inventorySlots;

    public GameObject m_warningSign; //appears if player tries to equip for example a weapon in legs-slot

    public void Start()
    {
        var player = CharManager.GetLocalPlayer();
        if (player == null)
            Debug.LogError("Could not find local player for slot");
        m_equipment = player.GetComponent<Equipment>();
		m_inventory = player.GetComponent<Inventory>();
        m_playerID = player.ID;
        if (m_equipment == null)
            Debug.LogError("Could not find equipment for slot");
        m_audioSource = gameObject.AddComponent<AudioSource>();
		m_inventorySlots = GameObject.FindGameObjectWithTag("InventorySlots");
		if(m_inventorySlots == null)
			Debug.LogError("Could not find inventory slots for slot");
    }

    public void EquipItem(GameObject itemName)
    {
        var item = itemName.GetComponent<Item>();
        SyncManager.AddEquipOrder(item.ID, m_playerID, true);
		m_inventory.m_items.Remove(itemName);
        m_audioSource.PlayOneShot(m_equipSound);
    }

    public void UnequipItem(GameObject itemName)
    {
        var item = itemName.GetComponent<Item>();
        SyncManager.AddEquipOrder(item.ID, m_playerID, false);
		m_inventory.m_items.Add(itemName);
    }

	public Slot GetEquipmentSlot(Item.ItemType type)
	{
		if (m_isInventory)
			Debug.LogError("Tried to get equip slots for inventory slot");
		var slots = transform.parent.GetComponentsInChildren<Slot>();
		for(int i = 0; i < slots.Length; ++i)
		{
			var slot = slots[i];
			if (slot.m_itemType == type)
				return slot;
		}
		return null;
	}

    public void OnDrop(PointerEventData eventData)
    {
        Draggable item = eventData.pointerDrag.GetComponent<Draggable>();
        if (item != null && !item.m_isDraggedButton)
		{
			if(item.m_itemType == Item.ItemType.SHIELD)
			{
				var weaponSlot = GetEquipmentSlot(Item.ItemType.WEAPON);
				if(weaponSlot.m_containsItem)
				{
					var weaponSlotItem = weaponSlot.GetComponentsInChildren<Draggable>()[0];
					if (weaponSlotItem.m_twoHandedWeapon)
						return;
				}
			}
			item.m_returnTo.GetComponent<Slot>().m_containsItem = false;
			if (m_itemType == item.m_itemType)
            {
                if (!m_containsItem)
                {
                    m_containsItem = true;
                }
                else if(m_containsItem)
                {
                    var oldEquippedItem = transform.GetChild(transform.childCount - 1);
                    oldEquippedItem.SetParent(item.m_returnTo.transform);
					item.m_returnTo.GetComponent<Slot>().m_containsItem = true;
					UnequipItem(oldEquippedItem.gameObject);
                }
				if(item.m_twoHandedWeapon)
				{
					var shieldSlot = GetEquipmentSlot(Item.ItemType.SHIELD);
					if(shieldSlot.m_containsItem)
					{
						var shieldItem = shieldSlot.GetComponentsInChildren<Item>()[0];
						var inventorySlots = m_inventorySlots.GetComponentsInChildren<Slot>();
						for(int i = 0; i < inventorySlots.Length; ++i)
						{
							var inventorySlot = inventorySlots[i];
							if(!inventorySlot.m_containsItem)
							{
								shieldItem.transform.SetParent(inventorySlot.transform);
								inventorySlot.m_containsItem = true;
								break;
							}
						}
						UnequipItem(shieldItem.gameObject);
					}
				}
                EquipItem(item.gameObject);
                item.m_returnTo = transform;
            }
            else if (m_isInventory)
            {
                var oldSlot = item.m_returnTo.GetComponent<Slot>();

                if (oldSlot.m_isInventory && !m_containsItem) //for moving an item to a different slot in inventory
                {
                    oldSlot.m_containsItem = false;
                    item.m_returnTo = transform;
                    m_containsItem = true;
                }
                else if (oldSlot.m_isInventory && m_containsItem && transform.childCount > 0) //for swapping two items in inventory
                {
                    var swapItem = transform.GetChild(0).GetComponent<Draggable>();
					if(swapItem != null)
					{
						swapItem.m_returnTo = oldSlot.transform;
						item.m_returnTo = transform;
						swapItem.transform.SetParent(swapItem.m_returnTo);
					}
                }

                else if (!oldSlot.m_isInventory && !m_containsItem) //for unequipping items
                {
                    oldSlot.m_containsItem = false;
                    item.m_returnTo = transform;
                    m_containsItem = true;
                    UnequipItem(item.gameObject);
                }
                else if (!oldSlot.m_isInventory && m_containsItem) //for swapping items by dragging from equipment to inventory
                {
                    var swapEquippedItem = transform.GetChild(0).GetComponent<Item>();
                    if (swapEquippedItem.m_typeOfItem == oldSlot.m_itemType)
                    {
                        UnequipItem(item.gameObject);
                        EquipItem(swapEquippedItem.gameObject);
                        swapEquippedItem.GetComponent<Draggable>().m_returnTo = oldSlot.transform;
                        swapEquippedItem.transform.SetParent(oldSlot.transform);
                        item.m_returnTo = transform;
                    }
                }
            }
            else
            {
                m_warningSign.SetActive(true);
            }
        }
    }
}