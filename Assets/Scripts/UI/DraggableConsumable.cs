using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DraggableConsumable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform m_returnTo; 

    private GameObject m_consumableButton;
    private GameObject m_dragButton = null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_consumableButton = new GameObject();
        m_consumableButton.AddComponent<RectTransform>();
        LayoutElement element = m_consumableButton.AddComponent<LayoutElement>();
        element.preferredHeight = GetComponent<LayoutElement>().preferredHeight;
        element.preferredWidth = GetComponent<LayoutElement>().preferredWidth;
        element.flexibleHeight = 0;
        element.flexibleWidth = 0;
        var canvasGroup = m_consumableButton.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        m_consumableButton.name = "test";
        var button = m_consumableButton.AddComponent<Button>();
        button.onClick.AddListener(() => { GetComponent<HealthPack>().UseHealthPack(); });
        var image = m_consumableButton.AddComponent<Image>();
        image.sprite = GetComponent<Image>().sprite;
        button.targetGraphic = image;
        m_consumableButton.AddComponent<DraggableConsumable>();
        m_dragButton = (GameObject)Instantiate(m_consumableButton, eventData.position, Quaternion.identity);
        m_dragButton.transform.SetParent(transform.parent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        m_dragButton.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_dragButton.transform.SetParent(m_returnTo);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
