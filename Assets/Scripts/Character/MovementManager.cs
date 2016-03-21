using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MovementManager : MonoBehaviour
{
	public static void InputMoveOrder(Vector2i targetGridPos) // when mouse is clicked, send move order for all local player objects (should be only one)
	{
		for (int i = 0; i < CharManager.Objects.Count; ++i)
		{
			var mover = CharManager.Objects[i].m_mover;
			if (mover.m_syncer.IsLocalPlayer())
			{
				mover.InputMoveOrder(targetGridPos);
				return;
			}
		}
	}

	public static void InputAttackOrder(int targetID)
	{
		for (int i = 0; i < CharManager.Objects.Count; ++i)
		{
			var controller = CharManager.Objects[i];
			var mover = controller.m_mover;
			if (mover.m_syncer.IsLocalPlayer())
			{
				//SyncManager.AddAttackOrder(targetID, controller.ID);
				ActionTargetData target = new ActionTargetData();
				target.m_playerTarget = true;
				target.m_targetID = targetID;
				ActionData actionData = new ActionData();
				actionData.m_actionID = mover.m_moveAction.ID;
				actionData.m_target = target;
				SyncManager.AddAction(actionData);
				mover.m_orderType = SimpleCharacterMovement.OrderType.attack;
				return;
			}
		}
	}

	//public static void OrderMove(MoveOrder order) // pass move order to the mover it is associated with by ID
	//{
	//	var mover = CharManager.GetObject(order.m_moverID).m_mover;
	//	if (mover == null)
	//		return;
	//	mover.MoveCommand(order.m_targetGridPos);
	//}

	//public static void OrderMoveVisualize(MoveOrder order) // pass move visualization order to the mover it is associated with by ID
	//{
	//	var controller = CharManager.GetObject(order.m_moverID);
	//	var mover = controller.m_mover;
	//	if (mover == null)
	//		return;
	//	mover.VisualizeMove(order.m_targetGridPos);
	//}

	//public static void OrderAttack(AttackOrder order)
	//{
	//	var mover = CharManager.GetObject(order.m_moverID).m_mover;
	//	if (mover == null)
	//		return;
	//	mover.AttackCommand(order.m_targetID);
	//}


	public static void RunServerTurn() // run server side game logic for all movers, eg. walk along the path determined by path finding
	{
		for (int i = 0; i < CharManager.Objects.Count; ++i)
		{
			var controller = CharManager.Objects[i];
			var combat = controller.m_combatSystem;
			if (combat.m_currentHp <= 0)
				continue;
            var mover = controller.m_mover;

			if (!controller.m_isPlayer) // If this character is an NPC, run its turn decision logic
				controller.m_enemyAI.TakeTurn();

			List<ActionData> combatVisualization = new List<ActionData>();
			bool moved = mover.TakeStep(ref combatVisualization);

			if (moved)
			{
				mover.m_syncer.SyncPosition(mover.m_gridPos);
				ActionTargetData target = new ActionTargetData();
				target.m_gridTarget = mover.m_gridPos;
				ActionData actionData = new ActionData();
				actionData.m_actionID = mover.m_visualizeMoveAction.ID;
				actionData.m_target = target;
				SyncManager.AddVisualizeAction(actionData);
				//SyncManager.AddMoveVisualizationOrder(mover.m_gridPos, controller.ID);
			}

			for(int j = 0; j < combatVisualization.Count; ++j)
				SyncManager.AddVisualizeAction(combatVisualization[j]);
		}
	}

	public static void RunClientTurn() // run client side logic for all movers. sends the next segment of move order for all player-controlled objects
	{
		for (int i = 0; i < CharManager.Objects.Count; ++i)
		{
			var controller = CharManager.Objects[i];
			var mover = controller.m_mover;
			if (mover.isLocalPlayer)
			{
				Vector2i nextTarget = Vector2i.Zero;
				bool moved = mover.GetNextMoveSegment(ref nextTarget);
				if (moved)
				{
					ActionTargetData target = new ActionTargetData();
					target.m_gridTarget = nextTarget;
					target.m_playerTarget = false;
					ActionData actionData = new ActionData();
					actionData.m_actionID = mover.m_moveAction.ID;
					actionData.m_target = target;
					SyncManager.AddAction(actionData, false);
				}
				//	SyncManager.AddMoveOrder(nextTarget, controller.ID);
			}
		}
	}

}
