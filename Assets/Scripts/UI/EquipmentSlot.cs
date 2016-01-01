using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{

    public Item.ItemType m_itemType = Item.ItemType.OTHER;
    
    private bool m_containsItem = false;
    private Equipment m_equipment;
    private int m_playerID;

    public GameObject m_warningSign; //appears if player tries to equip for example a weapon in legs-slot

    public void Start()
    {
        var player = CharManager.GetLocalPlayer();
        if (player == null)
            Debug.LogError("Could not find local player for equipmentslot");
        m_equipment = player.GetComponent<Equipment>();
        m_playerID = player.ID;
        if (m_equipment == null)
            Debug.LogError("Could not find equipment for equipmentslot");
    }

    public void EquipItem(GameObject item)
    {
        SyncManager.AddEquipOrder(item.GetComponent<Item>().ID, m_playerID, true);
    }

    public void UnequipItem(GameObject item)
    {
        SyncManager.AddEquipOrder(item.GetComponent<Item>().ID, m_playerID, false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Draggable item = eventData.pointerDrag.GetComponent<Draggable>();
        if (item != null)
        {
            if (m_itemType == item.m_itemType)
            {
                if(!m_containsItem)
                {
                    m_containsItem = true;
                }
                else
                {
                    var oldEquippedItem = transform.GetChild(transform.childCount - 1);
                    oldEquippedItem.SetParent(item.m_returnTo.transform);
                    UnequipItem(oldEquippedItem.gameObject);
                }
                EquipItem(item.gameObject);
                item.m_returnTo.GetComponent<InventorySlot>().m_containsItem = false;
                item.m_returnTo = transform;
            }
            else if(m_itemType == Item.ItemType.INVENTORY)
            {
                var oldEquipmentSlot = item.m_returnTo.GetComponent<EquipmentSlot>();
                var oldInventorySlot = item.m_returnTo.GetComponent<InventorySlot>();
                if(oldEquipmentSlot != null)
                {
                    oldEquipmentSlot.m_containsItem = false;
                    UnequipItem(item.gameObject);
                }
                if(oldInventorySlot != null)
                {
                    if(!GetComponent<InventorySlot>().m_containsItem)
                    {
                        oldInventorySlot.GetComponent<InventorySlot>().m_containsItem = false;
                    }
                }
                item.m_returnTo = transform;
                GetComponent<InventorySlot>().m_containsItem = true;
            }
            //TODO: fix case of dragging from equipment to correct item in inventory
            else
            {
                m_warningSign.SetActive(true);
            }
        }
    }
}