using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientTurnLogicManager : MonoBehaviour {

	private static List<ActionData> sm_actions = new List<ActionData>();
	private int m_nextAction = 0;
	private static bool sm_visualizationRunning = false;
	private static bool sm_lastActionFinished = true;

	// Use this for initialization
	void Start () {
	
	}

	void Update()
	{
		if (!sm_visualizationRunning || !sm_lastActionFinished)
			return;

		for (int i = m_nextAction; i < sm_actions.Count; ++i)
		{
			var action = ActionManager.GetAction(sm_actions[i].m_actionID);
			if (action == null)
				Debug.LogError("Unable to find action by ID: " + sm_actions[i].m_actionID);
			sm_lastActionFinished = false;
			action.Use(sm_actions[i].m_target);
			m_nextAction = i + 1;
			if (!sm_lastActionFinished)
				return;
		}
		m_nextAction = 0;
		sm_actions.Clear();
		FinishClientTurn();
	}

	public void StartClientTurnLogic(List<ActionData> actions)
	{
		m_nextAction = 0;
		sm_actions = actions;
		sm_lastActionFinished = true;
		sm_visualizationRunning = true;
		//StartCoroutine(RunVisualization());
		//MarkActionFinished();
	}

	public static void MarkActionFinished()
	{
		sm_lastActionFinished = true;
	}

	public void FinishClientTurn()
	{
		sm_visualizationRunning = false;
		SyncManager.OnClientSendVisualizeDone();
	}
}
