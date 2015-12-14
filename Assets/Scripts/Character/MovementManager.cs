﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MovementManager : MonoBehaviour
{
	private static List<SimpleCharacterMovement> sm_objects = new List<SimpleCharacterMovement>(); // Private list for iterating over all objects
	private static Dictionary<int, SimpleCharacterMovement> sm_objectDictionary = new Dictionary<int, SimpleCharacterMovement>(); // Private dictionary for fast item fetching by ID. Use GetObject to access this.

	public static ReadOnlyCollection<SimpleCharacterMovement> Objects { get { return sm_objects.AsReadOnly(); } } // Public property exposes only read-only version of list to prevent modification

	public void OnLevelWasLoaded(int level) // Reset all containers and sm_curID when loading new scene
	{
		Reset();
	}

	public static void Register(SimpleCharacterMovement item) // Add new object to objects list
	{
		sm_objects.Add(item);
		sm_objectDictionary.Add(item.ID, item);
	}

	public static void Unregister(int ID) // Remove object by ID.
	{
		var obj = GetObject(ID);
		if (obj == null)
			return;
		sm_objectDictionary.Remove(ID);
		sm_objects.Remove(obj);
	}

	public static SimpleCharacterMovement GetObject(int ID) // get mover associated with given ID
	{
		if (!sm_objectDictionary.ContainsKey(ID)) // if ID not found in dictionary, something's wrong
		{
			Debug.Log("Object ID not found from dictionary, ID: " + ID);
			return null;
		}
		return sm_objectDictionary[ID];
	}

	public static void InputMoveOrder(Vector2i targetGridPos) // when mouse is clicked, send move order for all local player objects (should be only one)
	{
		for (int i = 0; i < sm_objects.Count; ++i)
		{
			var mover = sm_objects[i];
			if (mover.m_syncer.IsLocalPlayer())
			{
				mover.InputMoveOrder(targetGridPos);
			}
		}
	}

	public static void InputAttackOrder(int targetID)
	{
		for (int i = 0; i < sm_objects.Count; ++i)
		{
			var mover = sm_objects[i];
			if (mover.m_syncer.IsLocalPlayer())
			{
				SyncManager.AddAttackOrder(targetID, mover.ID);
				mover.m_orderType = SimpleCharacterMovement.OrderType.attack;
				return;
			}
		}
	}

	public static void OrderMove(MoveOrder order) // pass move order to the mover it is associated with by ID
	{
		var mover = GetObject(order.m_moverID);
		if (mover == null)
			return;
		mover.MoveCommand(order.m_targetGridPos);
	}

	public static void OrderMoveVisualize(MoveOrder order) // pass move visualization order to the mover it is associated with by ID
	{
		var mover = GetObject(order.m_moverID);
		if (mover == null)
			return;
		mover.VisualizeMove(order.m_targetGridPos);
	}

	public static void OrderAttack(AttackOrder order)
	{
		var mover = GetObject(order.m_moverID);
		if (mover == null)
			return;
		mover.AttackCommand(order.m_targetID);
	}
	public static void KillObject(int m_targetID)
	{
		var mover = GetObject(m_targetID);
		if (mover == null)
			return;
		var combatSystem = mover.GetComponent<CombatSystem>();
		combatSystem.Die();
	}

    public static SimpleCharacterMovement GetLocalPlayer()
    {
        for(int i = 0; i < sm_objects.Count; ++i)
        {
            var mover = sm_objects[i];
            if (mover.isLocalPlayer)
                return mover;
        }
        return null;
    }

	public static void RunServerTurn() // run server side game logic for all movers, eg. walk along the path determined by path finding
	{
		for (int i = 0; i < sm_objects.Count; ++i)
		{
			var mover = sm_objects[i];
			bool moved = mover.TakeStep();

			if (moved)
			{
				mover.m_syncer.SyncPosition(mover.m_gridPos);
				SyncManager.AddMoveVisualizationOrder(mover.m_gridPos, mover.ID);
			}
		}
	}

	public static void RunClientTurn() // run client side logic for all movers. sends the next segment of move order for all player-controlled objects
	{
		for (int i = 0; i < sm_objects.Count; ++i)
		{
			var mover = sm_objects[i];
			if (mover.isLocalPlayer)
			{
				Vector2i nextTarget = Vector2i.Zero;
				bool moved = mover.GetNextMoveSegment(ref nextTarget);
				if (moved)
					SyncManager.AddMoveOrder(nextTarget, mover.ID);
			}
		}
	}

	private static void Reset()
	{
		sm_objectDictionary = new Dictionary<int, SimpleCharacterMovement>();
		sm_objects = new List<SimpleCharacterMovement>();
	}
}
