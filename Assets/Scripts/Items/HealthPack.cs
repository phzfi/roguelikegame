using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class HealthPack : MonoBehaviour
{
    public int m_heals = 1;

    public void UseHealthPack()
    {
        var player = CharManager.GetLocalPlayer();
        var combatSystem = player.GetComponent<CombatSystem>();
        combatSystem.m_currentHp = Mathf.Min(combatSystem.m_maxHp, combatSystem.m_currentHp + m_heals);
        var inventory = player.GetComponent<Inventory>();
        for(int i = 0; i < inventory.m_items.Count; ++i)
        {
            var item = inventory.m_items[i].GetComponent<Item>();
            if(item.m_name == "Potion")
            {
                inventory.m_items.Remove(item.gameObject);
                inventory.UpdatePotionCount();
                if(inventory.AmountOfPotions() < 1)
                    Destroy(gameObject);
                return;
            }
        }
        
    }
}
