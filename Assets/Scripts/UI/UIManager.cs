using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject m_inventoryPanel;
    public GameObject m_equipmentPanel;
    public GameObject m_wrongSlotWarningSign;
    public Text m_coinsText;

    private bool m_inventoryOpen = false;
    private bool m_equipmentOpen = false;
    private CharController m_localPlayer;
    private Inventory m_localInventory;

    void Update()
    {
        if (m_localPlayer == null)
            m_localPlayer = CharManager.GetLocalPlayer();
        else if(m_localPlayer != null)
        {
            m_localInventory = m_localPlayer.GetComponent<Inventory>();
            UpdateAmountOfCoins();
        }

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

    public void CloseWarningSign()
    {
        m_wrongSlotWarningSign.SetActive(false);
    }

    public void UpdateAmountOfCoins()
    {
        m_coinsText.text = m_localInventory.m_amountOfCoins.ToString();
    }
}