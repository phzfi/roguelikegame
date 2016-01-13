using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class HealthPack : MonoBehaviour
{
    public int m_heals = 1;
    

    public void UseHealthPack(GameObject button)
    {
        var player = CharManager.GetLocalPlayer();
        var inventory = player.GetComponent<Inventory>();
        var combatSystem = player.GetComponent<CombatSystem>();
        combatSystem.m_currentHp = Mathf.Min(combatSystem.m_maxHp, combatSystem.m_currentHp + m_heals);
        int amountOfPotions = inventory.AmountOfItem("Potion");
        if (amountOfPotions > 0)
        {
            for (int i = 0; i < inventory.m_items.Count; ++i)
            {
                var item = inventory.m_items[i].GetComponent<Item>();
                if (item.m_name == "Potion")
                {
                    inventory.m_items.Remove(item.gameObject);
                    amountOfPotions -= 1;
                    inventory.UpdatePotionCount();
                    Debug.Log("Health potion used.");
                    if (amountOfPotions < 1)
                    {
                        var inventorySlot = transform.GetComponentInParent<Slot>();
                        inventorySlot.m_containsItem = false;
                        Destroy(gameObject);
                        var actionBarSlot = button.transform.GetComponentInParent<ActionBarSlot>();
                        if(actionBarSlot.m_draggedButton != null)
                        {
                            Destroy(actionBarSlot.m_draggedButton);
                            actionBarSlot.m_isEmpty = true;
                        }
                        
                    }
                    return;
                }
            }
        }
        
        
    }
}
