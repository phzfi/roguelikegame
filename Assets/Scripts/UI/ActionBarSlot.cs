using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class ActionBarSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //DraggableConsumable consumable = eventData.pointerDrag.GetComponent<DraggableConsumable>();
        //if(consumable != null)
        //{
            
        //    if (consumable.m_returnTo == null)
        //        Destroy(consumable.gameObject);
        //    else
        //        consumable.m_returnTo = transform;
        //}
    }

    
}
