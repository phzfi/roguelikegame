﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Slot : MonoBehaviour, IDropHandler
{
    public bool m_isInventory = false;
    public Item.ItemType m_itemType = Item.ItemType.OTHER;
    public bool m_containsItem = false;

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
                if (!m_containsItem)
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