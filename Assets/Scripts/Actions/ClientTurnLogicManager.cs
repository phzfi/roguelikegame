using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientTurnLogicManager : MonoBehaviour {

	private static List<ActionData> sm_actions = new List<ActionData>();

	// Use this for initialization
	void Start () {
	
	}

	public static void StartClientTurnLogic(List<ActionData> actions)
	{
		sm_actions = actions;
		RunNextAction();
	}

	public static void RunNextAction()
	{
		if (sm_actions.Count == 0)
		{
			FinishClientTurn();
			return;
		}

		var data = sm_actions[0];
		sm_actions.RemoveAt(0);
        var action = ActionManager.GetAction(data.m_actionID);
		if (action == null)
			Debug.LogError("Unable to find action by ID: " + data.m_actionID);

		action.Use(data.m_target);
	}

	public static void FinishClientTurn()
	{
		Debug.Log("Sent visualize finish msg");
		SyncManager.OnClientSendVisualizeDone();
	}
}
