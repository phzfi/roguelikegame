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
	[HideInInspector]
	public bool m_twoHandedWeapon;

	private GameObject m_slots;
    private GameObject m_inventoryCanvas;

	//Maybe find a better idea for getting the game object "Slots", or even make a better solution for slots
	void Start()
	{
        m_inventoryCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas");
        if (GetComponent<Item>() != null)
		{
			var item = GetComponent<Item>();
            m_itemType = item.m_typeOfItem;
			m_twoHandedWeapon = item.m_twoHandedWeapon && item.m_typeOfItem == Item.ItemType.WEAPON;
		}

		var panels = m_inventoryCanvas.transform.GetChild(0);
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
        if (eventData.button == PointerEventData.InputButton.Left)
            transform.position = eventData.position;
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_returnTo = transform.parent;
            transform.SetParent(m_inventoryCanvas.transform);
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
					var slot = child.GetComponent<Slot>();
                    if (slot != null)
                    {
                        if (slot.m_itemType == m_itemType)
                        {
                            outline.effectColor = new Color(0, 1, 0, 1);
                            outline.enabled = true;
                        }
						else if(m_twoHandedWeapon && slot.m_itemType == Item.ItemType.SHIELD)
						{
							outline.effectColor = new Color(1, 1, 0, 1);
							outline.enabled = true;
						}
						else
                        {
                            outline.effectColor = new Color(1, 0, 0, 1);
                            outline.enabled = true;
                        }
						if(m_itemType == Item.ItemType.SHIELD)
						{
							var weaponSlot = slot.GetEquipmentSlot(Item.ItemType.WEAPON);
							if(weaponSlot.m_containsItem)
                            {
								var weaponSlotItem = weaponSlot.GetComponentsInChildren<Draggable>()[0];
								if (weaponSlotItem.m_twoHandedWeapon)
									outline.effectColor = new Color(1, 0, 0, 1);
							}
						}
                    }
                }
            }
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
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
}