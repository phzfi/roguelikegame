using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//A class for testing inventory, does not include anything else but the adding to remove all exceptions
public class TestInventory : MonoBehaviour
{
    public List<GameObject> m_items = new List<GameObject>();
    public int m_maxItems = 5;
    public int m_coins = 0;

    public bool CanAddItem(GameObject item)
    {
        return m_items.Count < m_maxItems;
    }

    public bool AddItem(GameObject item)
    {
        var itemName = item.GetComponent<Item>().m_name;
        if (CanAddItem(item) && itemName != "Coins")
        {
            m_items.Add(item);
            return true;
        }
        else if (itemName == "Coins")
        {
            m_coins += 1;
            return true;
        }
        return false;
    }
}

public class TestHealthpack : MonoBehaviour
{
    public int m_heals = 1;

    public void UseHealthPack(GameObject player)
    {
        var combatSystem = player.GetComponent<CombatSystem>();
        combatSystem.ChangeHP(m_heals);
     }
}

public class TestEquipment : MonoBehaviour
{
    public int m_playerStrength = 0;
    public int m_playerVitality = 0;
    public void EquipItem(GameObject itemName, GameObject player)
    {
        TestInventory m_inventory = player.GetComponent<TestInventory>();
        var item = itemName.GetComponent<Item>();
        m_inventory.m_items.Remove(itemName);
        m_playerStrength += item.m_strength;
        m_playerVitality += item.m_vitality;
    }
}
