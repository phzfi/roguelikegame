using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionManager : MonoBehaviour
{

	public Action m_currentAction = null;
	public bool m_currentlyTargeting = false; // Whether we're currently targeting some action. TODO: change cursor to appropriate

	public static Dictionary<ulong, Action> sm_actionDictionary;

	private InputHandler m_inputHandler;

	// Use this for initialization
	void Start()
	{
		m_inputHandler = FindObjectOfType<InputHandler>();
		sm_actionDictionary = new Dictionary<ulong, Action>();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public static Action GetAction(ulong ID)
	{
		if (sm_actionDictionary.ContainsKey(ID))
			return sm_actionDictionary[ID];
		return null;
	}

	public void TargetPosition(Vector2i mouseGridPos)
	{
		m_currentlyTargeting = false;

		if (m_currentAction == null)
			return;

		switch (m_currentAction.m_targetingType)
		{
			case Action.ActionTargetingType.ground: // If targeting type is ground, call action with grid position as target
				{
					ActionTargetData targetData = new ActionTargetData();
					targetData.m_playerTarget = false;
					targetData.m_gridTarget = mouseGridPos;
					ActionData action = new ActionData();
					action.m_actionID = m_currentAction.ID;
					action.m_target = targetData;
					SyncManager.AddAction(action);
				}
				break;

			case Action.ActionTargetingType.ranged: // If targeting a player, find player in mouse grid position and call action with the player as target
				{
					for (int i = 0; i < CharManager.Objects.Count; ++i)
					{
						var target = CharManager.Objects[i];
						if (mouseGridPos == target.m_mover.m_gridPos)
						{
							ActionTargetData targetData = new ActionTargetData();
							targetData.m_playerTarget = true;
							targetData.m_targetID = target.ID;
							targetData.m_userID = CharManager.GetLocalPlayer().ID;
							ActionData action = new ActionData();
							action.m_actionID = m_currentAction.ID;
							action.m_target = targetData;
							SyncManager.AddAction(action);
							break;
						}
					}
				}
				break;

			default: break;
		}
	}
}
