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

    public GameObject m_slots;

    public enum ItemType { WEAPON, HEAD, BODY, LEGS, RING, OTHER, INVENTORY }; //possible types of items, inventory as well to drag items back from equipment

    public ItemType m_itemType = ItemType.OTHER;

    private int m_oldIndex;

    void Start()
    {
        m_oldIndex = transform.GetSiblingIndex();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        var pointer = eventData.pointerEnter;
        if (pointer != null)
        {
            if (pointer.GetComponent<Draggable>() != null)
            {
                m_changeIndex = pointer.GetComponent<Draggable>().transform.GetSiblingIndex();
                //m_oldIndex = m_changeIndex;
                //Debug.Log(m_changeIndex);
            }

            if (pointer.GetComponent<Draggable>() == null)
            {
                m_changeIndex = m_oldIndex;
            }
        }

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
        //Debug.Log(m_returnTo);
        //Debug.Log("change " + m_changeIndex);
        //Debug.Log(transform + " siblin1 " + transform.GetSiblingIndex());
        transform.SetParent(m_returnTo);
        ////does not work properly yet,
        ////idea is to change positions when dragged and released on top of another item
        //Debug.Log(transform + " siblin2 " + transform.GetSiblingIndex());
        //if (transform.GetSiblingIndex() < m_changeIndex)
        //{
        //    transform.SetSiblingIndex(m_changeIndex - 1);
        //}
        //else if (transform.GetSiblingIndex() >= m_changeIndex)
        //{
        //    transform.SetSiblingIndex(m_changeIndex + 1);
        //}
        //transform.SetSiblingIndex(m_changeIndex + 1);

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