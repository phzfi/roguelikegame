using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class HealthPack : MonoBehaviour
{
    public int m_heals = 1;
    public ActionDelegate m_useDelegate;

    public void Start()
    {
        m_useDelegate = new ActionDelegate(UseHealthPack);
        var action = GetComponent<Action>();
        action.m_useDelegate = m_useDelegate;
    }

    public void UseHealthPack(ActionTargetData data)
    {
        var player = CharManager.GetLocalPlayer();
        var combatSystem = player.GetComponent<CombatSystem>();
        combatSystem.Heal(m_heals);
        var inventory = player.GetComponent<Inventory>();
        for (int i = 0; i < inventory.m_items.Count; ++i)
        {
            var item = inventory.m_items[i].GetComponent<Item>();
            if (item.m_name == "Potion")
            {
                inventory.m_items.Remove(item.gameObject);
                inventory.UpdatePotionCount();
                int potions = inventory.AmountOfItem("Potion");
                Debug.Log("Health potion used. Potions left " + potions);
                if (potions < 1)
                {
                    transform.parent.GetComponent<Slot>().m_containsItem = false;
                    Destroy(gameObject);
                    var draggedButtons = GetComponent<ActionDraggedButton>().m_draggedButtons;
                    for (int j = 0; j < draggedButtons.Count; ++j)
                    {
                        var actionBarSlot = draggedButtons[j].GetComponentInParent<ActionBarSlot>();
                        if (actionBarSlot != null)
                        {
                            actionBarSlot.m_isEmpty = true;
                            var button = actionBarSlot.GetComponentInChildren<Draggable>();
                            Destroy(button.gameObject);
                        }
                        Destroy(draggedButtons[j].gameObject);
                    }



                }

                return;
            }
        }

    }
}