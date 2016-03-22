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

    public GameObject m_warningSign; //appears if player tries to equip for example a weapon in legs-slot

    public void Awake()
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
    }

    public void EquipItem(GameObject itemName)
    {
        var item = itemName.GetComponent<Item>();
        SyncManager.AddEquipOrder(item.ID, m_playerID, true);
		m_inventory.m_items.Remove(itemName);
        m_equipment.m_playerStrength += item.m_strength;
        m_audioSource.PlayOneShot(m_equipSound);
    }

    public void UnequipItem(GameObject itemName)
    {
        var item = itemName.GetComponent<Item>();
        SyncManager.AddEquipOrder(item.ID, m_playerID, false);
		m_inventory.m_items.Add(itemName);
		m_equipment.m_playerStrength -= item.m_strength;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Draggable item = eventData.pointerDrag.GetComponent<Draggable>();
        if (item != null && !item.m_isDraggedButton)
        {
            if (m_itemType == item.m_itemType)
            {
                if (!m_containsItem)
                {
                    m_containsItem = true;
                }
                else if(m_containsItem && transform.childCount > 1)
                {
                    var oldEquippedItem = transform.GetChild(transform.childCount - 1);
                    oldEquippedItem.SetParent(item.m_returnTo.transform);
                    UnequipItem(oldEquippedItem.gameObject);
                }
                EquipItem(item.gameObject);
                item.m_returnTo.GetComponent<Slot>().m_containsItem = false;
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