using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;

public struct ActionTargetData
{
	public bool m_playerTarget;
	public Vector2i m_gridTarget;
	public int m_targetID;
	public int m_userID;
}

public struct ActionData
{
	public ActionTargetData m_target;
	public int m_actionID;
	public enum ActionType { move = 0, attack, }
}

public delegate void ActionDelegate(ActionTargetData targetData);

public class Action : NetworkBehaviour {

	public enum ActionTargetingType { self = 0, ranged, target, ground }

	public ActionTargetingType m_targetingType;
	public bool m_targetEnemies, m_targetFriendlies, m_targetSelf;
	public float m_targetingMaxRange = 0;
	
	public ActionDelegate m_useDelegate;

	[SyncVar]
	public int ID = -1;

	private ActionManager m_actionManager;
	private static int sm_lastID = -1;
	private bool m_registered = false;

	public void Start()
	{
		m_actionManager = FindObjectOfType<ActionManager>();

		if (SyncManager.IsServer)
			ID = sm_lastID++;
	}

	public void Update()
	{
		if (!m_registered && ID >= 0)
		{
			ActionManager.sm_actionDictionary.Add(ID, this);
			m_registered = true;
		}
    }

	public void OnMouseClick()
	{
		if (m_targetingType == ActionTargetingType.self)
			Use(new ActionTargetData());
		else
		{
			m_actionManager.m_currentAction = this;
			m_actionManager.m_currentlyTargeting = true;
		}
	}
	
	public void Use(ActionTargetData target)
	{
		m_useDelegate(target); // Runs the delegate that points to an item's or spell's use method.
	}
}
