using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
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
                var slot = m_slots.transform.GetChild(i).GetComponent<Slot>();
                var image = slot.GetComponent<Image>();

                if (slot != null && image.color.a == 1)
                {
                    if (slot.m_itemType == m_itemType)
                        image.color = new Color(0, 1, 0, 1);
                    else if (m_twoHandedWeapon && slot.m_itemType == Item.ItemType.SHIELD)
                        image.color = new Color(1, 1, 0, 1);
                    else
                        image.color = new Color(1, 0, 0, 1);

                    if (m_itemType == Item.ItemType.SHIELD && !slot.m_isInventory) // Check that no two-handed weapon is equipped when equipping shield
                    {
                        var weaponSlot = slot.GetEquipmentSlot(Item.ItemType.WEAPON);
                        if (weaponSlot.m_containsItem)
                        {
                            var weaponSlotItem = weaponSlot.GetComponentsInChildren<Draggable>()[0];
                            if (weaponSlotItem.m_twoHandedWeapon)
                                image.color = new Color(1, 0, 0, 1);
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
                var slot = m_slots.transform.GetChild(i);
                if (slot.GetComponent<Image>().color.a == 1)
                {
                    var img = slot.GetComponent<Image>();
                    img.color = new Color(1, 1, 1, 1);
                }
            }

            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left)
        {
            if (SyncManager.CheckInputPossible(true))
            {
                var action = GetComponent<Action>();
                var item = GetComponent<Item>();
                if (action != null && item != null && item.m_typeOfItem == Item.ItemType.OTHER)
                {
                    action.OnMouseClick();
                }
                else
                {
                    if (item != null)
                    {
                        for (int i = 0; i < m_slots.transform.childCount; i++)
                        {
                            var slot = m_slots.transform.GetChild(i).GetComponent<Slot>();
                            if (slot.m_itemType == item.m_typeOfItem)
                            {
                                if (slot.m_containsItem && slot.transform.childCount > 1)
                                {
                                    var inventorySlot = m_returnTo;
                                    var oldEquippedItem = slot.transform.GetChild(1).GetComponent<Draggable>();
                                    oldEquippedItem.m_returnTo = m_returnTo;
                                    m_returnTo = slot.transform;
                                    transform.SetParent(m_returnTo);
                                    oldEquippedItem.transform.SetParent(oldEquippedItem.m_returnTo);
                                    slot.UnequipItem(oldEquippedItem.gameObject);
                                }
                                else
                                {
                                    m_returnTo.GetComponent<Slot>().m_containsItem = false;
                                    m_returnTo = slot.transform;
                                    slot.GetComponent<Slot>().m_containsItem = true;
                                    transform.SetParent(m_returnTo);
                                }
                                slot.EquipItem(item.gameObject);
                                var sprite = item.GetComponent<Image>();
                                Color col = sprite.color;
                                if (!UIManager.sm_equipmentOpen)
                                    col.a = 0f;
                                sprite.color = col;
                                if(m_twoHandedWeapon)
                                {
                                    var shieldSlot = slot.GetEquipmentSlot(Item.ItemType.SHIELD);
                                    var shieldItem = slot.GetComponentsInChildren<Item>()[0];
                                    if (shieldItem == null)
                                        break;
                                    for(int j = 0; j < m_slots.transform.childCount; ++j)
                                    {
                                        var inventorySlot = m_slots.transform.GetChild(j).GetComponent<Slot>();
                                        if(!inventorySlot.m_containsItem)
                                        {
                                            var shield = shieldItem.GetComponent<Draggable>();
                                            shield.m_returnTo = inventorySlot.transform;
                                            shieldItem.transform.SetParent(shield.m_returnTo);
                                            shieldSlot.m_containsItem = false;
                                            inventorySlot.m_containsItem = true;
                                            slot.UnequipItem(shieldItem.gameObject);
                                            break;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            eventData.clickCount = 0;
        }
    }
}