
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
				if (!m_isEmpty)
				{
					var oldSlottedItem = GetComponentInChildren<Draggable>();
					Destroy(oldSlottedItem.gameObject);
				}
				var drag = button.GetComponent<Draggable>();

				drag.m_returnTo = transform;
				drag.transform.SetParent(drag.m_returnTo);
				m_isEmpty = false;
				var rectTransform = button.GetComponent<RectTransform>();
				rectTransform.localScale = Vector3.one;
			}

		}
	}

	private GameObject MakeUsableButton(GameObject original)
	{
		GameObject usableButton = new GameObject();
		var rectTransform = usableButton.AddComponent<RectTransform>();
		LayoutElement element = usableButton.AddComponent<LayoutElement>();
		LayoutElement oldElement = original.GetComponent<LayoutElement>();
		element.preferredHeight = oldElement.preferredHeight;
		element.preferredWidth = oldElement.preferredWidth;
		element.flexibleHeight = 0;
		element.flexibleWidth = 0;
		element.minHeight = oldElement.minHeight;
		element.minWidth = oldElement.minWidth;
		var canvasGroup = usableButton.AddComponent<CanvasGroup>();
		usableButton.name = original.name + " Button";
		var button = usableButton.AddComponent<Button>();
		var action = original.GetComponent<Action>();
		if (action != null)
			button.onClick.AddListener(() => { action.OnMouseClick(); });
		m_draggedButton = usableButton;

		var image = usableButton.AddComponent<Image>();
		image.sprite = original.GetComponent<Image>().sprite;
		button.targetGraphic = image;
		var drag = usableButton.AddComponent<Draggable>();
		drag.m_isDraggedButton = true;


		return usableButton;
	}
}