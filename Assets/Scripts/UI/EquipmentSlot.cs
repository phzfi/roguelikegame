using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{

    public Draggable.ItemType m_itemType = Draggable.ItemType.OTHER;

    public GameObject m_inventory;
    
    private bool m_containsItem = false;

    public GameObject m_warningSign; //appears if player tries to equip for example a weapon in legs-slot

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
                }
                item.m_returnTo = transform;
            }
            else if(m_itemType == Draggable.ItemType.INVENTORY)
            {
                var oldSlot = item.m_returnTo.GetComponent<EquipmentSlot>();
                if(oldSlot != null)
                {
                    oldSlot.m_containsItem = false;
                }
                item.m_returnTo = transform;
            }
            else
            {
                m_warningSign.SetActive(true);
            }
        }
    }
}