using UnityEngine;
using System.Collections;

public class BossAI : EnemyAI {
	//public int m_chaseRange = 5;
	//TODO: Store starting position and try to move there when no valid targets are in range.

	override public void TakeTurn() // Runs the turn decision logic for this NPC. 
	{
		float minDist = m_chaseRange;
		var target = m_controller;
		bool foundTarget = false;

		for(int i = 0; i < CharManager.Objects.Count; ++i) // Find closest player character inside attack range and attack it.
		{
			var controller = CharManager.Objects[i];
			if (!controller.m_isPlayer)
				continue;

			var mover = controller.m_mover;
			var myPos = m_mover.m_gridPos;
			var enemyPos = mover.m_gridPos;
			float dist = myPos.Distance(enemyPos);

			if(dist < minDist && controller.m_enemyAI == null) // TODO: line of sight and some smarter parameters
			{
				minDist = dist;
				target = controller;
				foundTarget = true;
			}
		}

		if(foundTarget) // If target found, chase and attack it
		{
			ActionTargetData targetdata = new ActionTargetData();
			targetdata.m_playerTarget = true;
			targetdata.m_targetID = target.ID;
			m_mover.MoveCommand(targetdata);
		}
	}
}
