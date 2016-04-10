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

    private GameObject m_equipmentSlots;
    private GameObject m_inventorySlots;
    private GameObject m_inventoryCanvas;

    //Maybe find a better idea for getting the game object "Slots", or even make a better solution for slots
    void Start()
    {
        m_equipmentSlots = GameObject.FindGameObjectWithTag("Equipment");
        m_inventoryCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas");
        m_inventorySlots = GameObject.FindGameObjectWithTag("InventorySlots");
        if (GetComponent<Item>() != null)
        {
            var item = GetComponent<Item>();
            m_itemType = item.m_typeOfItem;
            m_twoHandedWeapon = item.m_twoHandedWeapon && item.m_typeOfItem == Item.ItemType.WEAPON;
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

            for (int i = 0; i < m_equipmentSlots.transform.childCount; i++)
            {
                var slot = m_equipmentSlots.transform.GetChild(i).GetComponent<Slot>();
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

            for (int i = 0; i < m_equipmentSlots.transform.childCount; i++)
            {
                var slot = m_equipmentSlots.transform.GetChild(i);
                if (slot.GetComponent<Image>().color.a == 1)
                {
                    var img = slot.GetComponent<Image>();
                    img.color = new Color(1, 1, 1, 1);
                }
            }

            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    private bool UnequipShield(GameObject shield, GameObject shieldSlot)
    {
        for(int i = 0; i < m_inventorySlots.transform.childCount; i++)
        {
            var slot = m_inventorySlots.transform.GetChild(i);
            if (slot.GetComponent<Slot>().m_containsItem)
                continue;
            shield.transform.SetParent(slot);
            shield.GetComponent<Draggable>().m_returnTo = slot.transform;
            slot.GetComponent<Slot>().m_containsItem = true;
            shieldSlot.GetComponent<Slot>().m_containsItem = false;
            return true;
        }
        return false;
    }

    private bool Unequip2h(GameObject twohand, GameObject weaponSlot)
    {
        for (int i = 0; i < m_inventorySlots.transform.childCount; i++)
        {
            var slot = m_inventorySlots.transform.GetChild(i);
            if (slot.GetComponent<Slot>().m_containsItem)
                continue;
            twohand.transform.SetParent(slot);
            twohand.GetComponent<Draggable>().m_returnTo = slot.transform;
            slot.GetComponent<Slot>().m_containsItem = true;
            weaponSlot.GetComponent<Slot>().m_containsItem = false;
            return true;
        }
        return false;
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
                        for (int i = 0; i < m_equipmentSlots.transform.childCount; i++)
                        {
                            var slot = m_equipmentSlots.transform.GetChild(i).GetComponent<Slot>();
                            if (slot.m_itemType == item.m_typeOfItem)
                            {
                                var shieldSlot = slot.GetEquipmentSlot(Item.ItemType.SHIELD);
                                var shieldItem = shieldSlot.GetComponentInChildren<Item>();

                                var weaponSlot = slot.GetEquipmentSlot(Item.ItemType.WEAPON);
                                var weaponItem = weaponSlot.GetComponentInChildren<Item>();

                                var inventorySlot = m_returnTo;
                                if (m_twoHandedWeapon)
                                {
                                    if(shieldItem != null)
                                    {
                                        if(UnequipShield(shieldItem.gameObject, shieldSlot.gameObject))
                                            shieldSlot.UnequipItem(shieldItem.gameObject);
                                    }
                                }
                                else
                                {
                                    if(m_itemType == Item.ItemType.SHIELD)
                                    {
                                        if(weaponItem != null)
                                        {
                                            if(weaponItem.m_twoHandedWeapon)
                                            {
                                                if (Unequip2h(weaponItem.gameObject, weaponSlot.gameObject))
                                                    weaponSlot.UnequipItem(weaponItem.gameObject);
                                            }
                                        }
                                    }
                                }
                                if (slot.m_containsItem && slot.transform.childCount > 0)
                                {
                                    var oldEquippedItem = slot.transform.GetChild(1).GetComponent<Draggable>();
                                    oldEquippedItem.m_returnTo = inventorySlot;
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
                            }
                        }
                    }
                }
            }
            eventData.clickCount = 0;
        }
    }
}