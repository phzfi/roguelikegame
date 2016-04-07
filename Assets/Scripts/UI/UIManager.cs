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
    public GameObject m_tooltip;
    public ActionBarSlot[] m_actionBarSlots;

    public static bool sm_inventoryOpen = false;
    public static bool sm_equipmentOpen = false;
    private CharController m_localPlayer;
    private Inventory m_localInventory;
    
    void Update()
	{
		if (SyncManager.IsDedicatedServer)
			return;

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
            m_healthBar.value = (float) playerCombatSystem.m_visualizeHp / playerCombatSystem.m_maxHp;
            
            if (m_localPlayer != null)
                m_playerNameText.text = m_localPlayer.m_name.ToString(); //player's ID is used as player's name for now
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseHotkey(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseHotkey(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            UseHotkey(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            UseHotkey(4);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            UseHotkey(5);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            UseHotkey(6);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            UseHotkey(7);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            UseHotkey(8);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            UseHotkey(9);
    }

    void ToggleInventoryAlpha(float alpha)
    {
        var inventory = m_inventoryPanel.GetComponentsInChildren<Image>();
        var texts = m_inventoryPanel.GetComponentsInChildren<Text>();
        for (int i = 0; i < inventory.Length; i++)
        {
            Color col = inventory[i].color;
            col.a = alpha;
            inventory[i].color = col;
        }
        for (int i = 0; i < texts.Length; i++)
        {
            Color col = texts[i].color;
            col.a = alpha;
            texts[i].color = col;
        }
    }

    void ToggleEquipmentAlpha(float alpha)
    {
        var equipment = m_equipmentPanel.GetComponentsInChildren<Image>();
        var texts = m_equipmentPanel.GetComponentsInChildren<Text>();
        for (int i = 0; i < equipment.Length; i++)
        {
            Color col = equipment[i].color;
            col.a = alpha;
            equipment[i].color = col;
        }
        for (int i = 0; i < texts.Length; i++)
        {
            Color col = texts[i].color;
            col.a = alpha;
            texts[i].color = col;
        }
    }

    public void ToggleInventory()
    {
        if(!ChatManager.sm_chatOpen)
        {
            if (!sm_inventoryOpen)
            {
                ToggleInventoryAlpha(1f);
                sm_inventoryOpen = true;
            }
            else if (sm_inventoryOpen)
            {
                ToggleInventoryAlpha(0f);
                sm_inventoryOpen = false;
                if (Tooltip.sm_tooltipOpen)
                {
                    m_tooltip.SetActive(false);
                    Tooltip.sm_tooltipOpen = false;
                }
                    
            }
        }
        
    }

    public void ToggleEquipment()
    {
        if (!ChatManager.sm_chatOpen)
        {
            if (!sm_equipmentOpen)
            {
                ToggleEquipmentAlpha(1f);
                sm_equipmentOpen = true;
            }
            else if (sm_equipmentOpen)
            {
                ToggleEquipmentAlpha(0f);
                sm_equipmentOpen = false;
                if (m_tooltip.activeInHierarchy)
                    m_tooltip.SetActive(false);
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

    public void UseHotkey(int button)
    {
        var clickable = m_actionBarSlots[button - 1].GetComponentInChildren<Button>();
        if (clickable != null)
            clickable.onClick.Invoke();
    }
}