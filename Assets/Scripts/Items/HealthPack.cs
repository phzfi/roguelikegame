using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HealthPack : MonoBehaviour
{
    public int m_heals = 1;
    public ActionDelegate m_useDelegate;

	private Action m_healAction;

    public void Start()
    {
        m_useDelegate = new ActionDelegate(UseHealthPack);
        var action = GetComponent<Action>();
        action.m_useDelegate = m_useDelegate;

		m_healAction = gameObject.AddComponent<Action>();
		m_healAction.Initialize();
		m_healAction.m_useDelegate = HealOnServer;
    }

	public void HealOnServer(ActionTargetData data)
	{
		var player = CharManager.GetObject(data.m_targetID);
		var combatSystem = player.GetComponent<CombatSystem>();
		List<ActionData> visualization = new List<ActionData>();
		combatSystem.Heal(m_heals, ref visualization);

		for (int i = 0; i < visualization.Count; ++i)
		{
			var actionData = visualization[i];
			SyncManager.AddVisualizeAction(actionData);
		}
	}

    public void UseHealthPack(ActionTargetData data)
    {
        var player = CharManager.GetLocalPlayer();

		if (!SyncManager.CheckInputPossible(true))
			return;

		ActionData action = new ActionData();
		ActionTargetData target = new ActionTargetData();
		target.m_targetID = player.ID;
		action.m_actionID = m_healAction.ID;
		action.m_target = target;

		SyncManager.AddAction(action); // Tell server to run hitpoint adjustment

		var inventory = player.GetComponent<Inventory>();
        for (int i = 0; i < inventory.m_items.Count; ++i) // Remove potion from local inventory
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