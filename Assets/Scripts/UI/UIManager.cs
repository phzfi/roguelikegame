using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject m_inventoryPanel;
    public GameObject m_equipmentPanel;
    public GameObject m_wrongSlotWarningSign;
    public Text m_coinsText;
    public Text m_playerNameText;
    public Slider m_healthBar;

    public static bool sm_inventoryOpen = false;
    public static bool sm_equipmentOpen = false;
    private CharController m_localPlayer;
    private Inventory m_localInventory;

    void Update()
    {
        if (m_localPlayer == null)
        {
            m_localPlayer = CharManager.GetLocalPlayer();
        }
        if(m_localInventory == null && m_localPlayer != null)
        {
            m_localInventory = m_localPlayer.GetComponent<Inventory>();
        }
        if (m_localPlayer != null && m_localInventory != null)
        {
            UpdateAmountOfCoins();
            var playerCombatSystem = m_localPlayer.GetComponent<CombatSystem>();
            m_healthBar.value = (float) playerCombatSystem.m_currentHp / playerCombatSystem.m_maxHp;
            
            if (m_localPlayer != null)
                m_playerNameText.text = "Player: " + m_localPlayer.ID.ToString(); //player's ID is used as player's name for now
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
        if(!ChatManager.sm_chatOpen)
        {
            if (!sm_inventoryOpen)
            {
                m_inventoryPanel.SetActive(true);
                sm_inventoryOpen = true;
            }
            else if (sm_inventoryOpen)
            {
                m_inventoryPanel.SetActive(false);
                sm_inventoryOpen = false;
            }
        }
        
    }

    public void ToggleEquipment()
    {
        if (!ChatManager.sm_chatOpen)
        {
            if (!sm_equipmentOpen)
            {
                m_equipmentPanel.SetActive(true);
                sm_equipmentOpen = true;
            }
            else if (sm_equipmentOpen)
            {
                m_equipmentPanel.SetActive(false);
                sm_equipmentOpen = false;
            }
        }
    }

    private void ToggleBoth()
    {
        if((!sm_equipmentOpen && !sm_inventoryOpen) || (sm_equipmentOpen && sm_inventoryOpen))
        {
            ToggleEquipment();
            ToggleInventory();
        }
        else if(!sm_equipmentOpen && sm_inventoryOpen)
        {
            ToggleEquipment();
        }
        else if(sm_equipmentOpen && !sm_inventoryOpen)
        {
            ToggleInventory();
        }
        
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