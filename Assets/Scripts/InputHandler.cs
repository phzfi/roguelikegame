using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputHandler : Singleton<InputHandler>
{
	protected InputHandler() { }

	public Transform selectedTile;
	public Material selectedTileMaterial;
	public Color hoverColor = Color.black;
	public Color selectedColor = Color.red;

	public PathVisualization pathVisualization;
	private ActionManager m_actionManager;

	static readonly Plane sm_groundPlane = new Plane(new Vector3(0, 0, -1), new Vector3(0, 0, 0));

	void Start()
	{
		if (selectedTile)
		{
			selectedTile.localScale = new Vector3(MapGrid.tileSize, MapGrid.tileSize, 1.0f);
		}

		m_actionManager = FindObjectOfType<ActionManager>();
	}

	void Update()
	{
		if (MenuManager.sm_menuOpen)
			return;

		Vector2i mouseGridPos = GetMouseGridPosition();

		// Visualize tile that mouse cursor hovers over
		if (selectedTile)
		{
			selectedTile.position = MapGrid.GridToWorldPoint(mouseGridPos, -0.06f);

			if (selectedTileMaterial)
			{
				selectedTileMaterial.color = Input.GetMouseButton(1) ? selectedColor : hoverColor;
			}            
		}

		// Visualize potential movement path to target tile
		if (pathVisualization && !m_actionManager.m_currentlyTargeting)
		{
			List<Vector3> worldSpacePath = new List<Vector3>();
			
			if (Input.GetMouseButton(1))
			{
				var mover = CharManager.GetLocalPlayer();
				if (mover == null)
					Debug.Log("Inputhandler could not find local player");

				NavPath navPath = mover.m_mover.m_navAgent.SeekPath(mover.m_mover.m_gridPos, mouseGridPos);
				worldSpacePath = MapGrid.NavPathToWorldSpacePath(navPath, -0.07f);
            }

            if (worldSpacePath.Count >= 2)
			{
				pathVisualization.Create(worldSpacePath);
			}
			else
			{
				pathVisualization.Hide();
			}
        }

		if (Input.GetKeyDown(KeyCode.Escape)) // Esc cancels action targeting
			m_actionManager.m_currentlyTargeting = false;

		if (Input.GetMouseButtonUp(1))
		{
			if (m_actionManager.m_currentlyTargeting) // If action targeting is active, use action instead of attacking or moving
			{
				m_actionManager.TargetPosition(mouseGridPos);
			}
			else
			{
				bool attack = false;
				for (int i = 0; i < CharManager.Objects.Count; ++i)
				{
					var target = CharManager.Objects[i];
					if (mouseGridPos == target.m_mover.m_gridPos)
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
	}
	
	public static Vector2i GetMouseGridPosition()
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
