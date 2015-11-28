using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputHandler : Singleton<InputHandler>
{
	protected InputHandler() { }

	static readonly Plane sm_groundPlane = new Plane(new Vector3(0, 0, -1), new Vector3(0, 0, 0));

	void Update()
	{
		if (Input.GetMouseButtonUp(1))
		{
			Vector2i mouseGridPos = GetMouseGridPosition();

			bool attack = false;
			for (int i = 0; i < MovementManager.Objects.Count; ++i)
			{
				var target = MovementManager.Objects[i];
				if (mouseGridPos == target.m_gridPos)
				{
					attack = true;
					MovementManager.InputAttackOrder(target.ID);
					break;
				}
			}

			if (!attack)
			{
				MovementManager.InputMoveOrder(mouseGridPos);
			}
		}
	}

	void OnDrawGizmos()
	{
		// TODO proper visualizing
		if (Input.GetMouseButton(1))
		{
			Gizmos.color = Color.red;
			Vector2i mouseGridPos = GetMouseGridPosition();
			Gizmos.DrawWireCube(MapGrid.GridToWorldPoint(mouseGridPos), new Vector3(MapGrid.tileSize, MapGrid.tileSize, 0.1f));
		}
	}

	private static Vector2i GetMouseGridPosition()
	{
		Debug.Assert(Camera.main, "Main camera not found!");

		Ray worldSpaceRay = Camera.main.ScreenPointToRay(Input.mousePosition);

		float hitDistance = 0.0f;
		if (sm_groundPlane.Raycast(worldSpaceRay, out hitDistance))
		{
			Vector3 mouseWorldPos = worldSpaceRay.origin + worldSpaceRay.direction * hitDistance;
			return MapGrid.WorldToGridPoint(mouseWorldPos);
		}
		else
		{
			Debug.LogError("Unable to convert mouse position to grid position!");
			return MapGrid.WorldToGridPoint(0, 0);
		}
	}
}
