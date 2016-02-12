﻿
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class ActionBarSlot : MonoBehaviour, IDropHandler
{
	public int m_slotNumber = 1;
	public GameObject m_outOfItemSign;
	public GameObject m_draggedButton;
	public bool m_isEmpty = true;

	void Start()
	{
		m_slotNumber = transform.GetSiblingIndex() + 1;
	}

	public void OnDrop(PointerEventData eventData)
	{
		Draggable item = eventData.pointerDrag.GetComponent<Draggable>();
		if (item != null)
		{
			if (item.m_itemType == Item.ItemType.OTHER)
			{
				GameObject button = MakeUsableButton(item.gameObject);
                GameObject addButton = Instantiate(button);
				if (!m_isEmpty)
				{
					var oldSlottedItem = GetComponentInChildren<Draggable>();
					Destroy(oldSlottedItem.gameObject);
				}
				var drag = addButton.GetComponent<Draggable>();

				drag.m_returnTo = transform;
				drag.transform.SetParent(drag.m_returnTo);
				m_isEmpty = false;
				var rectTransform = addButton.GetComponent<RectTransform>();
				rectTransform.localScale = Vector3.one;
                Debug.Log(ActionDraggedButton.m_draggedButtons.Count);
			}

		}
	}

    //Makes a button based on the item/spell that was dragged to the slot.
	private GameObject MakeUsableButton(GameObject original)
	{
		GameObject usableButton = new GameObject();
		LayoutElement element = usableButton.AddComponent<LayoutElement>();
		LayoutElement oldElement = original.GetComponent<LayoutElement>();
		element.preferredHeight = oldElement.preferredHeight;
		element.preferredWidth = oldElement.preferredWidth;
		element.flexibleHeight = 0;
		element.flexibleWidth = 0;
		element.minHeight = oldElement.minHeight;
		element.minWidth = oldElement.minWidth;
		usableButton.name = original.name + " Button";
		var button = usableButton.AddComponent<Button>();
		var action = original.GetComponent<Action>();
		if (action != null)
			button.onClick.AddListener(() => { action.OnMouseClick(); });
        ActionDraggedButton.m_draggedButtons.Add(usableButton);        
		//m_draggedButton = usableButton;

		var image = usableButton.AddComponent<Image>();
		image.sprite = original.GetComponent<Image>().sprite;
		button.targetGraphic = image;
		var drag = usableButton.AddComponent<Draggable>();
		drag.m_isDraggedButton = true;
        usableButton.AddComponent<CanvasGroup>();

		return usableButton;
	}
}