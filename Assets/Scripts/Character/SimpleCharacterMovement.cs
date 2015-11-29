using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SimpleCharacterMovement : NetworkBehaviour
{
	public enum OrderType { none, move, attack }
	public OrderType m_orderType;
	public Vector2i m_moveOrderTarget;
	public int m_attackOrderTarget;

	public int ID;
	public int m_gridSpeed = 6;

	public Vector2i m_gridPos;
	public PlayerSync m_syncer;
	private AudioSource m_audioSource;
	
	public NavPathAgent m_navAgent;
	public NavPath m_navPath = new NavPath();
	private NavPath m_moveOrderPath = new NavPath();

	private CombatSystem m_combatSystem;

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
		Debug.Assert(m_navAgent.CanAccess(m_gridPos), "Character " + gameObject.name + ", ID: " + ID + " is in unaccessable location");
		
		transform.position = MapGrid.GridToWorldPoint(m_gridPos, transform.position.z);

		ID = (int)netId.Value;
		MovementManager.Register(this);
	}

	public void OnDestroy()
	{
		Unregister();
	}

	public void Unregister()
	{
		MovementManager.Unregister(ID);
	}

	public void InputMoveOrder(Vector2i targetGridPos) // Update the move order. Run pathfinding to move target. 
	{
		m_moveOrderPath = m_navAgent.SeekPath(m_gridPos, targetGridPos);
		m_orderType = OrderType.move;
	}

	public bool GetNextMoveSegment(ref Vector2i targetGridPos) // Get the next move segment containing one turn's movement. This will get sent to the server as a move order.
	{
		if (m_orderType != OrderType.move) // if the object doesn't currently have a move order, return
			return false;

		int currentMoveIndex = Mathf.Min(m_moveOrderPath.Count, m_gridSpeed) - 1;
		if (currentMoveIndex < 0)
		{
			return false;
		}

		targetGridPos = m_moveOrderPath[currentMoveIndex];
		m_moveOrderPath.RemoveRange(0, currentMoveIndex + 1);
		return true;
	}

	public void MoveCommand(Vector2i targetGridPos) // Tell this object to start moving towards new target
	{
		m_orderType = OrderType.move;
		m_moveOrderTarget = targetGridPos;
	}

	public void AttackCommand(int targetID)
	{
		m_orderType = OrderType.attack;
		m_attackOrderTarget = targetID;
	}

	public void VisualizeMove(Vector2i targetGridPos) // Start movement visualization towards given point
	{
		// TODO should we use world coordinates here? Or store start grid pos to move command?
		Vector2i startGridPos = MapGrid.WorldToGridPoint(transform.position);

		NavPath tempPath = m_navAgent.SeekPath(startGridPos, targetGridPos);
		if (tempPath.Count == 0)
			return;

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

		StopAllCoroutines(); // kill previous interpolations if they're still going

		if (currentPath.Count >= 2)
		{
			List<Vector3> worldSpacePath = new List<Vector3>(currentPath.Count);
			for (int i = 0; i < currentPath.Count; ++i)
			{
				worldSpacePath.Add(MapGrid.GridToWorldPoint(currentPath[i], transform.position.z));
			}

			CatmullRomSpline spline = new CatmullRomSpline(worldSpacePath);
			StartCoroutine(InterpolateCurveMovementCoroutine(spline, currentPath.Count));
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
				var target = MovementManager.GetObject(m_attackOrderTarget);
				if (target == null) // if target not found, it must be dead and we can cancel attack order
				{
					m_orderType = OrderType.none;
					return false;
				}
				m_navPath = m_navAgent.SeekPath(m_gridPos, target.m_gridPos);
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
			for (int i = 0; i < MovementManager.Objects.Count; ++i) // loop over objects to check next path step is not blocked
			{
				var mover = MovementManager.Objects[i];
				if (mover == this)
					continue;

				if (mover.m_gridPos == nextGridPos)
				{
					// TODO: do something smart here, don't just stop?
					var combatSystem = mover.GetComponent<CombatSystem>();
					if (combatSystem != null)
					{
						m_combatSystem.Attack(mover.ID);
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

				for (int i = 0; i < ItemManager.ItemsOnMap.Count; ++i)
				{
					var item = ItemManager.ItemsOnMap[i];
					if (item.m_pos == m_gridPos && item.CanPickup(gameObject))
						SyncManager.AddPickupOrder(ID, item.ID);
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
