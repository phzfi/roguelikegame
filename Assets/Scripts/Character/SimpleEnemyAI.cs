using UnityEngine;
using System.Collections;

public class SimpleEnemyAI : EnemyAI {

    public int m_steps = 1;

    private int m_turn = 0;

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
        else if (m_mover.m_orderType == SimpleCharacterMovement.OrderType.attack && !foundTarget)
        {
            ActionTargetData targetdata = new ActionTargetData();
            targetdata.m_playerTarget = false;
            targetdata.m_gridTarget = m_mover.m_gridPos;
            m_mover.MoveCommand(targetdata);
        } else
        {
            ActionTargetData targetdata = new ActionTargetData();
            targetdata.m_playerTarget = false;
            Vector2i movePos = m_mover.m_gridPos;

            if (m_turn < 1 * m_steps) movePos.x++;
            else if (m_turn < 2 * m_steps) movePos.y++;
            else if (m_turn < 3 * m_steps) movePos.x--;
            else if (m_turn < 4 * m_steps) movePos.y--;

            targetdata.m_gridTarget = movePos;
            m_mover.MoveCommand(targetdata);
            m_turn++;
            m_turn %= 4 * m_steps;
        }
    }
}
