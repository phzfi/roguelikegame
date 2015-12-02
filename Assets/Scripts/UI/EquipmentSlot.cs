using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{

    public Draggable.ItemType m_itemType = Draggable.ItemType.OTHER;

    public GameObject m_warningSign; //appears if player tries to equip for example a weapon in legs-slot

    public void OnDrop(PointerEventData eventData)
    {
        Draggable item = eventData.pointerDrag.GetComponent<Draggable>();
        if (item != null)
        {
            if (m_itemType == item.m_itemType || m_itemType == Draggable.ItemType.INVENTORY)
            {
                //item.transform.SetSiblingIndex(item.m_changeIndex);
                item.m_returnTo = transform;
            }
        }
    }
}