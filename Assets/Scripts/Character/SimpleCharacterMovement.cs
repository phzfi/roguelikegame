using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SimpleCharacterMovement : NetworkBehaviour
{
	public enum OrderType { none, move, attack }
	[HideInInspector]
	public OrderType m_orderType = OrderType.none;
	[HideInInspector]
	public Vector2i m_moveOrderTarget;
	[HideInInspector]
	public int m_attackOrderTarget;

	public int m_gridSpeed = 6;

	public Vector2i m_gridPos;
	[HideInInspector]
	public PlayerSync m_syncer;
	private AudioSource m_audioSource;
	public Action m_moveAction;
	public Action m_visualizeMoveAction;
	
	public NavPathAgent m_navAgent;
	public NavPath m_navPath = new NavPath();
	private NavPath m_moveOrderPath = new NavPath();

	private CombatSystem m_combatSystem;
	private CharController m_controller;
    private CharacterAnimation m_animator;

	bool m_onGoingMovement = false;
	float m_distanceOnStep = 0.0f;
	float m_visualizationSpeed = 4.0f;
	float m_visualizationRotationSpeed = 6.0f;
	int m_step = 0;

	void Start()
	{
		m_audioSource = GetComponent<AudioSource>();
		m_syncer = GetComponent<PlayerSync>();
		m_combatSystem = GetComponent<CombatSystem>();

		LevelMapManager mapManager = FindObjectOfType<LevelMapManager>();
		LevelMap map = mapManager.GetMap();
		NavGrid navGrid = map.GetNavGrid();

		m_navAgent = new NavPathAgent(0.5f, navGrid, new MovementEvalFuncDelegate(d => d.f));

		m_gridPos = MapGrid.WorldToGridPoint(transform.position);
		Debug.Assert(m_navAgent.CanAccess(m_gridPos), "Character " + gameObject.name + " is in unaccessable location");
		
		transform.position = MapGrid.GridToWorldPoint(m_gridPos, transform.position.z);
        m_animator = GetComponent<CharacterAnimation>();

		m_controller = GetComponent<CharController>();

		m_moveAction = gameObject.AddComponent<Action>();
		m_moveAction.Initialize();
		m_visualizeMoveAction = gameObject.AddComponent<Action>();
		m_visualizeMoveAction.Initialize();
		m_moveAction.m_useDelegate = MoveCommand;
		m_visualizeMoveAction.m_useDelegate = VisualizeMove;
	}

	public void InputMoveOrder(Vector2i target) // Update the move order. Run pathfinding to move target. 
	{
		m_moveOrderPath = m_navAgent.SeekPath(m_gridPos, target);
		m_orderType = OrderType.move;
	}

	public bool GetNextMoveSegment(ref Vector2i targetGridPos) // Get the next move segment containing one turn's movement. This will get sent to the server as a move order.
	{
		if (m_orderType != OrderType.move) // if the object doesn't currently have a move order, return
			return false;

        int offset = 0;
        if (m_moveOrderPath.Count > 0 && m_gridPos != m_moveOrderPath[0]) // adjust movement for difference in path speed (first segment of pathfinding result is always starting square)
            offset = -1;
		int currentMoveIndex = Mathf.Min(m_moveOrderPath.Count, m_gridSpeed + offset) - 1;
		if (currentMoveIndex < 0)
		{
			return false;
		}

		targetGridPos = m_moveOrderPath[currentMoveIndex];
		m_moveOrderPath.RemoveRange(0, currentMoveIndex + 1);
		return true;
	}

	public void MoveCommand(ActionTargetData target) // Tell this object to start moving towards new target
	{
		if (!target.m_playerTarget)
		{
		m_orderType = OrderType.move;
			m_moveOrderTarget = target.m_gridTarget;
	}
		else
	{
		m_orderType = OrderType.attack;
			m_attackOrderTarget = target.m_targetID;
		}
	}

	public void VisualizeMove(ActionTargetData target) // Start movement visualization towards given point
	{
		// TODO should we use world coordinates here? Or store start grid pos to move command?
		Debug.Log("visualizing move id: " + m_controller.ID);
		Vector2i startGridPos = MapGrid.WorldToGridPoint(transform.position);
		Vector2i targetGridPos = target.m_gridTarget;

		NavPath tempPath = m_navAgent.SeekPath(startGridPos, targetGridPos);
		if (tempPath.Count == 0)
		{
			ClientTurnLogicManager.RunNextAction();
			return;
		}

		NavPath currentPath = new NavPath();
		for (int i = 0; i < m_gridSpeed; ++i)
		{
			if (i == tempPath.Count)
				break;
			currentPath.Add(tempPath[i]);
		}

		m_distanceOnStep = .0f;
		m_step = 0;
		m_onGoingMovement = true;
        m_animator.ToggleWalkAnimation(true);

		StopAllCoroutines(); // kill previous interpolations if they're still going
		if (currentPath.Count >= 2)
		{
			List<Vector3> worldSpacePath = MapGrid.NavPathToWorldSpacePath(currentPath, transform.position.z);
			CatmullRomSpline spline = new CatmullRomSpline(worldSpacePath);
			StartCoroutine(InterpolateCurveMovementCoroutine(spline, spline.ControlPointCount));
		}
		else
		{
			Vector3 startWorldPos = MapGrid.GridToWorldPoint(startGridPos, transform.position.z);
			Vector3 endWorldPos = MapGrid.GridToWorldPoint(targetGridPos, transform.position.z);
			StartCoroutine(InterpolateTwoPointsLerpMovementCoroutine(startWorldPos, endWorldPos, true));
		}
    }

	IEnumerator InterpolateCurveMovementCoroutine(CatmullRomSpline spline, int pathPointCount)
	{
		while (m_onGoingMovement)
		{
			float d = m_visualizationSpeed * Time.deltaTime;
			m_distanceOnStep += d;
			while (m_distanceOnStep > MapGrid.tileSize)
			{
				m_distanceOnStep -= MapGrid.tileSize;
				spline.NextSection();
				m_step++;
			}

			if (m_step < (pathPointCount - 4))
			{
				Vector3 worldPos = spline.Interpolate(m_distanceOnStep / MapGrid.tileSize);
				Quaternion look = Quaternion.LookRotation(Vector3.forward, (spline.GetNextControlPoint() - transform.position).normalized);
				transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * m_visualizationRotationSpeed);
				transform.position = worldPos;
			}
			else
			{
				StartCoroutine(InterpolateTwoPointsLerpMovementCoroutine(transform.position, spline.GetLastControlPoint(), true));
				yield break;
			}
			yield return null;
		}
	}

	IEnumerator InterpolateTwoPointsLerpMovementCoroutine(Vector3 startWorldPos, Vector3 endWorldPos, bool rotation)
	{
		while (m_onGoingMovement)
		{
			Quaternion look = Quaternion.LookRotation(Vector3.forward, (endWorldPos - startWorldPos).normalized);
			float dist = Vector3.Distance(startWorldPos, endWorldPos);
			float d = m_visualizationSpeed * Time.deltaTime;
			if (rotation)
				transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * m_visualizationRotationSpeed);
			m_distanceOnStep += d;
			if (m_distanceOnStep >= dist)
			{
				transform.position = endWorldPos;
				m_onGoingMovement = false;
				m_animator.ToggleWalkAnimation(false);
				ClientTurnLogicManager.RunNextAction();
				yield break;
			}
			else
			{
				transform.position = Vector3.Lerp(startWorldPos, endWorldPos, m_distanceOnStep / dist);
			}
			yield return null;
		}
	}

	public bool TakeStep() // Move this object towards the current target, move m_gridSpeed steps
	{
		switch (m_orderType)
		{
			case OrderType.none:
				return false;
			case OrderType.move:
                m_navPath = m_navAgent.SeekPath(m_gridPos, m_moveOrderTarget);
                break;
			case OrderType.attack:
				var target = CharManager.GetObject(m_attackOrderTarget);
				if (target == null) // if target not found, it must be dead and we can cancel attack order
				{
					m_orderType = OrderType.none;
					return false;
				}
				m_navPath = m_navAgent.SeekPath(m_gridPos, target.m_mover.m_gridPos);
				break;
		}
		if (m_navPath.Count == 0)
			return false;

		bool moved = false;

		for (int step = 0; step < m_gridSpeed; step++)
		{
			if (m_navPath.Count == 0)
				return moved;

			Vector2i nextGridPos = m_navPath[0];

			bool movementBlocked = false;
			for (int i = 0; i < CharManager.Objects.Count; ++i) // loop over objects to check next path step is not blocked
			{
				var controller = CharManager.Objects[i];
				if (controller == m_controller)
					continue;

				if (controller.m_mover.m_gridPos == nextGridPos)
				{
					// TODO: do something smart here, don't just stop?
					var combatSystem = controller.GetComponent<CombatSystem>();
					if (combatSystem != null)
					{
						m_combatSystem.Attack(controller.ID);
					}
					movementBlocked = true;
					break;
				}
			}

			if (!movementBlocked) // if nextPos was not blocked, move there and remove one segment from path
			{
				m_gridPos = nextGridPos;
				m_navPath.RemoveAt(0);
				moved = true;

				if(m_controller.m_isPlayer)
					for (int i = 0; i < ItemManager.ItemsOnMap.Count; ++i) // pick up items from current grid square, but only if player character
					{
						var item = ItemManager.ItemsOnMap[i];
						if (item.m_pos == m_gridPos && item.CanPickup(gameObject))
							SyncManager.AddPickupOrder(m_controller.ID, item.ID);
					}
			}
			else
				return moved;
		}

		m_audioSource.Play();
		return moved;
	}

	// TODO proper visualizing
	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		for (int i = 1; i < m_moveOrderPath.Count; ++i)
		{
			Vector3 start = MapGrid.GridToWorldPoint(m_moveOrderPath[i - 1]);
			Vector3 end = MapGrid.GridToWorldPoint(m_moveOrderPath[i]);
			Gizmos.DrawLine(start, end);
		}
	}

}
