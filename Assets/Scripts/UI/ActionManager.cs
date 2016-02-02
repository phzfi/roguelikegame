using UnityEngine;
using System.Collections;

public class ActionManager : MonoBehaviour
{

	public Action m_currentAction = null;
	public bool m_currentlyTargeting = false; // Whether we're currently targeting some action. TODO: change cursor to appropriate

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

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
					m_currentAction.Use(targetData);
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
							m_currentAction.Use(targetData);
							break;
						}
					}
				}
				break;

			default: break;
		}
	}
}
