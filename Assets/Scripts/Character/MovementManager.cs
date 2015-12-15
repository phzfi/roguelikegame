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
				SyncManager.AddAttackOrder(targetID, controller.ID);
				mover.m_orderType = SimpleCharacterMovement.OrderType.attack;
				return;
			}
		}
	}

	public static void OrderMove(MoveOrder order) // pass move order to the mover it is associated with by ID
	{
		var mover = CharManager.GetObject(order.m_moverID).m_mover;
		if (mover == null)
			return;
		mover.MoveCommand(order.m_targetGridPos);
	}

	public static void OrderMoveVisualize(MoveOrder order) // pass move visualization order to the mover it is associated with by ID
	{
		var mover = CharManager.GetObject(order.m_moverID).m_mover;
		if (mover == null)
			return;
		mover.VisualizeMove(order.m_targetGridPos);
	}

	public static void OrderAttack(AttackOrder order)
	{
		var mover = CharManager.GetObject(order.m_moverID).m_mover;
		if (mover == null)
			return;
		mover.AttackCommand(order.m_targetID);
	}
	public static void KillObject(int m_targetID)
	{
		var mover = CharManager.GetObject(m_targetID).m_mover;
		if (mover == null)
			return;
		var combatSystem = mover.GetComponent<CombatSystem>();
		combatSystem.Die();
	}


	public static void RunServerTurn() // run server side game logic for all movers, eg. walk along the path determined by path finding
	{
		for (int i = 0; i < CharManager.Objects.Count; ++i)
		{
			var controller = CharManager.Objects[i];
            var mover = controller.m_mover;

			if (!controller.m_isPlayer) // If this character is an NPC, run its turn decision logic
				controller.m_enemyAI.TakeTurn();

			bool moved = mover.TakeStep();

			if (moved)
			{
				mover.m_syncer.SyncPosition(mover.m_gridPos);
				SyncManager.AddMoveVisualizationOrder(mover.m_gridPos, controller.ID);
			}
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
					SyncManager.AddMoveOrder(nextTarget, controller.ID);
			}
		}
	}

}
