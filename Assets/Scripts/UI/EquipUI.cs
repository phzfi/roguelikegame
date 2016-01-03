using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EquipUI : MonoBehaviour {

    private Slot[] m_inventorySlots;

    void Start()
    {
        m_inventorySlots = GetComponentsInChildren<Slot>();
    }

    public void HighlightCorrectSlots(Item item)
    {
        for (int i = 0; i < m_inventorySlots.Length; ++i)
        {
            var slot = m_inventorySlots[i].GetComponent<Item>();
            var outline = slot.GetComponent<Outline>();
            if (slot.m_typeOfItem == item.m_typeOfItem)
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

    public void ClearHighlight()
    {
        for (int i = 0; i < m_inventorySlots.Length; ++i)
        {
            var slot = m_inventorySlots[i];
            var outline = slot.GetComponent<Outline>();
            outline.enabled = false;
        }
    }
}
