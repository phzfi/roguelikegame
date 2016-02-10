using UnityEngine;
using System.Collections;

public class RangedAttack : MonoBehaviour {

	public float m_maxRange;
	public int m_damage;
	public ActionDelegate m_useDelegate;

	public void Start()
	{
		m_useDelegate = new ActionDelegate(Attack);
		var action = GetComponent<Action>();
		action.m_useDelegate = m_useDelegate;
	}

	public void Attack(ActionTargetData targetData)
	{
		CharController target = null;
		CharController source = CharManager.GetObject(targetData.m_userID);

		if (targetData.m_playerTarget)
		{
			target = CharManager.GetObject(targetData.m_targetID);
		}
		else
		{
			for(int i = 0; i < CharManager.Objects.Count; ++i)
			{
				var controller = CharManager.Objects[i];
				if(controller.m_mover.m_gridPos == targetData.m_gridTarget)
				{
					target = controller;
					break;
				}
			}
		}

		if (target == null || source == null) // If target not found or attacker invalid, do nothing
			return;

		if (!LineOfSight.CheckLOS(source.m_mover.m_navAgent, source.m_mover.m_gridPos, target.m_mover.m_gridPos, m_maxRange).blocked) // Check that target is visible
		{
			target.m_combatSystem.ChangeHP(-m_damage);

			//TODO: Effects etc.
		}
	}
}
