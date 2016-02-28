using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	[HideInInspector]
	public bool m_isDraggedButton = false;

	[HideInInspector]
	public Transform m_returnTo;

	[HideInInspector]
	public int m_changeIndex;

	[HideInInspector]
	public Item.ItemType m_itemType;

	private GameObject m_slots;

	//Maybe find a better idea for getting the game object "Slots", or even make a better solution for slots
	void Start()
	{
		if (GetComponent<Item>() != null)
		{
			m_itemType = GetComponent<Item>().m_typeOfItem;
		}

		var panels = GameObject.FindGameObjectWithTag("InventoryCanvas").transform.GetChild(0);
		for (int i = 0; i < panels.childCount; ++i)
		{
			var equipment = panels.transform.GetChild(i);
			if (equipment.tag == "Equipment")
			{
				var equipmentPanel = equipment.GetChild(0);
				for (int j = 0; j < equipmentPanel.childCount; ++j)
				{
					if (equipmentPanel.GetChild(j).name.Contains("Slots"))
					{
						m_slots = equipmentPanel.GetChild(j).gameObject;
						return;
					}
				}
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		transform.position = eventData.position;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{

		m_returnTo = transform.parent;
		transform.SetParent(transform.parent.parent);
		if (m_isDraggedButton)
		{
			LayoutElement element = GetComponent<LayoutElement>();
			element.ignoreLayout = true;
		}

		for (int i = 0; i < m_slots.transform.childCount; i++)
		{
			var child = m_slots.transform.GetChild(i);
			var outline = child.GetComponent<Outline>();
			if (outline != null)
			{
				if (child.GetComponent<Slot>() != null)
				{
					if (child.GetComponent<Slot>().m_itemType == m_itemType)
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
		if (m_isDraggedButton)
		{
			LayoutElement element = GetComponent<LayoutElement>();
			element.ignoreLayout = false;
		}

		for (int i = 0; i < m_slots.transform.childCount; i++)
		{
			var child = m_slots.transform.GetChild(i);
			var outline = child.GetComponent<Outline>();
			outline.enabled = false;
		}

		GetComponent<CanvasGroup>().blocksRaycasts = true;
	}

}