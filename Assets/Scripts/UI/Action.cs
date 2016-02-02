using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class ActionTargetData
{
	public bool m_playerTarget = false;
	public Vector2i m_gridTarget;
	public int m_targetID;
}

public delegate void ActionDelegate(ActionTargetData targetData);

public class Action : MonoBehaviour {

	public enum ActionTargetingType { self = 0, ranged, target, ground }

	public ActionTargetingType m_targetingType;
	public bool m_targetEnemies, m_targetFriendlies, m_targetSelf;
	public float m_targetingMaxRange = 0;
	
	public ActionDelegate m_useDelegate;

	private ActionManager m_actionManager;

	public void Start()
	{
		m_actionManager = FindObjectOfType<ActionManager>();
	}

	public void OnMouseClick()
	{
		if (m_targetingType == ActionTargetingType.self)
			Use(new ActionTargetData());
		else
			m_actionManager.m_currentAction = this;
	}
	
	public void Use(ActionTargetData target)
	{
		m_useDelegate(target); // Runs the delegate that points to an item's or spell's use method.
	}
}
