using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{

    public Item.Type m_itemType = Item.Type.OTHER;

    public GameObject m_inventory;
    
    private bool m_containsItem = false;

    public GameObject m_warningSign; //appears if player tries to equip for example a weapon in legs-slot

    public void EquipItem(GameObject item)
    {
        Equipment.sm_equipment.Add(item);
    }

    public void UnequipItem(GameObject item)
    {
        Equipment.sm_equipment.Remove(item);
        //m_inventory.GetComponent<Inventory>().AddItem(item); //TODO: Items to be returned back to inventory
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
                    oldEquippedItem.SetParent(m_inventory.transform);
                    UnequipItem(oldEquippedItem.gameObject);
                }
                EquipItem(item.gameObject);
                Debug.Log("Items equipped " + Equipment.sm_equipment.Count);
                item.m_returnTo = transform;
            }
            else if(m_itemType == Item.Type.INVENTORY)
            {
                var oldSlot = item.m_returnTo.GetComponent<EquipmentSlot>();
                if(oldSlot != null)
                {
                    oldSlot.m_containsItem = false;
                    UnequipItem(item.gameObject);
                }
                item.m_returnTo = transform;
            }
            else
            {
                m_warningSign.SetActive(true);
            }
            Debug.Log("Items equipped " + Equipment.sm_equipment.Count);
        }
    }
}