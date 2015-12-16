using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisibilityManager : MonoBehaviour
{

	public static TileRenderer sm_moveRangeRenderer;
	public static TileRenderer sm_openLosRenderer;
	public static TileRenderer sm_closedLosRenderer;

	private static LevelMapManager sm_mapManager;

	private enum VisibilityState { open = 0, visible, blocked}

	private static VisibilityState[] sm_visibilityGrid;
	private static Dictionary<Vector2i, bool> sm_openTiles;
	private static int w, h;

	public void Start()
	{
		sm_mapManager = FindObjectOfType<LevelMapManager>();
		var renderers = GetComponentsInChildren<TileRenderer>();
		for (int i = 0; i < renderers.Length; ++i)
		{
			if (renderers[i].m_type == TileRenderer.TileRendererType.Movement)
				sm_moveRangeRenderer = renderers[i];
			else if (renderers[i].m_type == TileRenderer.TileRendererType.FogOfWarOpen)
				sm_openLosRenderer = renderers[i];
			else if (renderers[i].m_type == TileRenderer.TileRendererType.FogOfWarClosed)
				sm_closedLosRenderer = renderers[i];
		}

		w = sm_mapManager.m_width;
		h = sm_mapManager.m_height;

		sm_visibilityGrid = new VisibilityState[w * h];
		sm_openTiles = new Dictionary<Vector2i, bool>();
	}

	public void Update()
	{
		if (SyncManager.IsTurnInProgress())
			sm_moveRangeRenderer.Hide();
		else
			sm_moveRangeRenderer.Show();

		UpdateMoveRange();
		UpdateLos();
	}

	public static void UpdateMoveRange()
	{
		sm_moveRangeRenderer.CreateMesh(BreadthFirstSearch().ToArray());
	}

	public static void UpdateLos()
	{
		Dictionary<Vector2i, bool> openTiles = new Dictionary<Vector2i, bool>();
		Dictionary<Vector2i, bool> visibleResultTiles = new Dictionary<Vector2i, bool>();
		Dictionary<Vector2i, bool> blockedResultTiles = new Dictionary<Vector2i, bool>();

		List<Vector3> resultsVisible = new List<Vector3>();
		List<Vector3> resultsBlocked = new List<Vector3>();

		var controller = CharManager.GetLocalPlayer();
		if (controller == null)
			return;

		var mover = controller.m_mover;
		var navAgent = mover.m_navAgent;
		Vector2i startTile = mover.m_gridPos;

		int sightRange = 30;

		Vector2i startCorner = new Vector2i(startTile.x - sightRange, startTile.y - sightRange);
		Vector2i stopCorner = new Vector2i(startTile.x + sightRange, startTile.y + sightRange);

		for (int x = startCorner.x; x < stopCorner.x; ++x) // First, add circle range of tiles to openTiles set
			for (int y = startCorner.y; y < stopCorner.y; ++y)
			{
				Vector2i currentTile = new Vector2i(x, y);
				if (currentTile.Distance(startTile) < sightRange + 1 && currentTile.Distance(startTile) > sightRange - 1)
					openTiles.Add(currentTile, false);
			}

		int passCount = 0;
		while (passCount < 2)
		{
			List<Vector2i> keys = new List<Vector2i>(openTiles.Keys);
			for (int i = 0; i < keys.Count; ++i) // Then, loop over all open tiles, casting visibility rays for each tile
			{
				Vector2i currentTile = keys[i];
				if (openTiles[currentTile] || blockedResultTiles.ContainsKey(currentTile))
					continue;

				var losResult = LineOfSight.CheckLOS(navAgent, startTile, currentTile, sightRange, true);

				if (losResult.blocked) // If los ray was blocked at some point
				{
					//resultsBlocked.Add(MapGrid.GridToWorldPoint(currentTile)); // Add tile to list of invisible grid coords
					for (int j = 0; j < losResult.blockedTiles.Count; ++j)
					{
						Vector2i tile = losResult.blockedTiles[j];
						if (openTiles.ContainsKey(tile)) // Mark visited any blocked tiles along the ray from the set of open tiles so that we won't have to visit it
							openTiles[tile] = true;

						if (!blockedResultTiles.ContainsKey(tile)) // Add the same tiles to list of invisible tile coordinates
						{
							blockedResultTiles.Add(tile, true);
							resultsBlocked.Add(MapGrid.GridToWorldPoint(tile));
						}
					}
					for (int j = 0; j < losResult.openTiles.Count; ++j)
					{
						Vector2i tile = losResult.openTiles[j];
						if (openTiles.ContainsKey(tile))
							openTiles[tile] = true;

						if (!visibleResultTiles.ContainsKey(tile))
						{
							resultsVisible.Add(MapGrid.GridToWorldPoint(tile));
							visibleResultTiles.Add(tile, true);
						}
					}
				}
				else // If ray wasn't blocked at all, add current tile to visible tile coordinate list
				{
					//if (!visibleResultTiles.ContainsKey(currentTile))
					//{
					//	resultsVisible.Add(MapGrid.GridToWorldPoint(currentTile));
					//	visibleResultTiles.Add(currentTile, true);
					//}
					for (int j = 0; j < losResult.openTiles.Count; ++j) // add visible tiles along the ray to visible coordinates list too
					{
						Vector2i tile = losResult.openTiles[j];
						if (openTiles.ContainsKey(tile))
							openTiles[tile] = true;

						if (!visibleResultTiles.ContainsKey(tile))
						{
							resultsVisible.Add(MapGrid.GridToWorldPoint(tile));
							visibleResultTiles.Add(tile, true);
						}
					}
				}
			}

			openTiles.Clear();
			for (int x = startCorner.x; x < stopCorner.x; ++x) // For the second pass, add any missed tiles to the open tiles set
				for (int y = startCorner.y; y < stopCorner.y; ++y)
				{
					Vector2i currentTile = new Vector2i(x, y);
					if (currentTile.Distance(startTile) < sightRange + 1 && !visibleResultTiles.ContainsKey(currentTile) && !blockedResultTiles.ContainsKey(currentTile))
						openTiles.Add(currentTile, false);
				}

			passCount++;
		}

		sm_closedLosRenderer.CreateMesh(resultsBlocked.ToArray());
		sm_openLosRenderer.CreateMesh(resultsVisible.ToArray());
	}

	public static List<Vector3> BreadthFirstSearch()
	{
		List<Vector3> result = new List<Vector3>();

		var player = CharManager.GetLocalPlayer();

		if (player == null)
			return result;

		var mover = player.m_mover;
		var navAgent = mover.m_navAgent;
		int speed = mover.m_gridSpeed;
		Vector2i start = mover.m_gridPos;

		Dictionary<Vector2i, bool> visitedTiles = new Dictionary<Vector2i, bool>();
		Queue<Vector3i> openTiles = new Queue<Vector3i>();
		openTiles.Enqueue(new Vector3i(start.x, start.y, 0));

		while (openTiles.Count > 0) // run breadth-first search for accessible grids in movement range
		{
			Vector3i curTile = openTiles.Dequeue();

			for (int i = 0; i < 8; ++i)
			{
				Vector3i adjacentTile = curTile;

				if (i < 4)
				{
					adjacentTile.x += (i % 2) * 2 - 1; // get diagonal tile offsets
					adjacentTile.y += (i / 2) * 2 - 1;
				}
				else
				{
					int j = i - 4;
					adjacentTile.x += (1 - j / 2) * ((j % 2) * 2 - 1); // get horizontal and vertical tile offsets
					adjacentTile.y += (j / 2) * ((j % 2) * 2 - 1);
				}
				adjacentTile.z += 1; // increment distance travelled by one

				Vector2i adjacentTileGrid = new Vector2i(adjacentTile.x, adjacentTile.y);
				if (!visitedTiles.ContainsKey(adjacentTileGrid) && adjacentTile.z < speed && navAgent.CanAccess(adjacentTileGrid))
				{
					result.Add(MapGrid.GridToWorldPoint(adjacentTileGrid));
					visitedTiles.Add(adjacentTileGrid, true);
					openTiles.Enqueue(adjacentTile);
				}
			}
		}

		return result;
	}
}
