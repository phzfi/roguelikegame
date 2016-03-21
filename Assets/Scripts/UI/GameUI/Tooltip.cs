using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject m_tooltip;

    [HideInInspector]
    public static bool sm_tooltipOpen = false;

    private Text m_itemName;
    private Text m_infoText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!eventData.dragging)
        {
            var tooltipTexts = m_tooltip.transform.GetChild(0);
            for (int i = 0; i < tooltipTexts.childCount; ++i)
            {
                if (tooltipTexts.GetChild(i).name.Contains("Name"))
                    m_itemName = tooltipTexts.GetChild(i).GetComponent<Text>();
                else
                    m_infoText = tooltipTexts.GetChild(i).GetComponent<Text>();
            }
            
            if (transform.childCount > 0)
            {
                var item = transform.GetChild(0).GetComponent<Item>();
                if(item != null)
                {
                    m_itemName.text = item.m_name;
                    m_infoText.text = item.ToString();

                    Vector3 pos = new Vector3(eventData.position.x - 500, eventData.position.y - 300); //broken
                    m_tooltip.transform.localPosition = pos;
                    m_tooltip.SetActive(true);
                    sm_tooltipOpen = true;
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_tooltip.SetActive(false);
    }

}
