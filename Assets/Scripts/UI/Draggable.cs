using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    [HideInInspector]
    public GameObject m_placeholderItem;

    [HideInInspector]
    public Transform m_returnTo;

    [HideInInspector]
    public Transform m_placeholderParent;

    [HideInInspector]
    public int m_changeIndex;
    
    [HideInInspector]
    public Item.ItemType m_itemType;

    private GameObject m_slots;

    //Maybe find a better idea for getting the game object "Slots", or even make a better solution for slots
    void Start()
    {
        m_itemType = GetComponent<Item>().m_typeOfItem;
        var inventoryCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas");
        for(int i = 0; i < inventoryCanvas.transform.childCount; ++i)
        {
            var equipment = inventoryCanvas.transform.GetChild(i);
            if (equipment.tag == "Equipment")
            {
                var equipmentPanel = equipment.GetChild(0);
                for(int j = 0; j < equipmentPanel.childCount; ++j)
                {
                    if(equipmentPanel.GetChild(j).name.Contains("Slots"))
                    {
                        m_slots = equipmentPanel.GetChild(j).gameObject;
                        break;
                    }
                }
                break;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_placeholderItem = new GameObject();
        m_placeholderItem.transform.SetParent(transform.parent);
        LayoutElement element = m_placeholderItem.AddComponent<LayoutElement>();
        element.preferredHeight = GetComponent<LayoutElement>().preferredHeight;
        element.preferredWidth = GetComponent<LayoutElement>().preferredWidth;
        element.flexibleHeight = 0;
        element.flexibleWidth = 0;

        m_placeholderItem.transform.SetSiblingIndex(transform.GetSiblingIndex());
        m_returnTo = transform.parent;
        m_placeholderParent = m_returnTo;
        transform.SetParent(transform.parent.parent);

        for (int i = 0; i < m_slots.transform.childCount; i++)
        {
            var child = m_slots.transform.GetChild(i);
            var outline = child.GetComponent<Outline>();
            if (outline != null)
            {
                if (child.GetComponent<EquipmentSlot>() != null)
                {
                    if (child.GetComponent<EquipmentSlot>().m_itemType == m_itemType)
                    {
                        outline.effectColor = new Color(0, 1, 0, 1);
                        outline.enabled = true;
                    }
                    else
                    {
                        outline.effectColor = new Color(1, 0, 0, 1);
                        outline.enabled = true;
                    }
                }
            }
        }
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(m_returnTo);
        
        for (int i = 0; i < m_slots.transform.childCount; i++)
        {
            var child = m_slots.transform.GetChild(i);
            var outline = child.GetComponent<Outline>();
            outline.enabled = false;
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        Destroy(m_placeholderItem);
    }

}