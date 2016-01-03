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

    private CharController m_localPlayer;
    private Inventory m_localInventory;

   //Maybe find a better idea for getting the game object "Slots", or even make a better solution for slots
   void Start()
    {
        m_itemType = GetComponent<Item>().m_typeOfItem;
        m_localPlayer = CharManager.GetLocalPlayer();
        if(m_localPlayer != null) 
            m_localInventory = m_localPlayer.GetComponent<Inventory>();   
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos = eventData.position;
        transform.position = pos;
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

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(m_returnTo);

        GetComponent<CanvasGroup>().blocksRaycasts = true;

        Destroy(m_placeholderItem);
    }

    

}