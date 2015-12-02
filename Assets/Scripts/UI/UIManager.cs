﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject m_inventoryPanel;
    public GameObject m_equipmentPanel;
    public Text m_coinsText;

    private bool m_inventoryOpen = false;
    private bool m_equipmentOpen = false;

    void Start()
    {
        m_coinsText.text = Inventory.sm_amountOfCoins.ToString();
    }

    void Update()
    {
        m_coinsText.text = Inventory.sm_amountOfCoins.ToString();
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleBoth();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleEquipment();
        }
    }

    public void ToggleInventory()
    {
        if (!m_inventoryOpen)
        {
            m_inventoryPanel.SetActive(true);
            m_inventoryOpen = true;
        }
        else if (m_inventoryOpen)
        {
            m_inventoryPanel.SetActive(false);
            m_inventoryOpen = false;
        }
    }

    public void ToggleEquipment()
    {
        if (!m_equipmentOpen)
        {
            m_equipmentPanel.SetActive(true);
            m_equipmentOpen = true;
        }
        else if (m_equipmentOpen)
        {
            m_equipmentPanel.SetActive(false);
            m_equipmentOpen = false;
        }
    }

    private void ToggleBoth()
    {
        ToggleEquipment();
        ToggleInventory();
    }

}