using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisibilityManager : MonoBehaviour
{

	public static TileRenderer sm_moveRangeRenderer;
	public static TileRenderer sm_openLosRenderer;
	public static TileRenderer sm_closedLosRenderer;

	private static LevelMapManager sm_mapManager;

	private enum VisibilityState { open = 0, visible, blocked }

	private static VisibilityState[] sm_visibilityGrid;
	private static Dictionary<Vector2i, bool> sm_openTiles;
	private static int w, h;
	private static Vector2i sm_playerPos;
	private static NavPathAgent sm_navAgent;

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

        w = 10; // sm_mapManager.;
        h = 10; // sm_mapManager.m_height;

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

	private static bool InBounds(Vector2i tile)
	{
		return tile.x >= 0 && tile.y >= 0 && tile.x < w && tile.y < h;
	}

	private static VisibilityState GetTile(Vector2i tile)
	{
		if (!InBounds(tile))
			return VisibilityState.blocked;
		return sm_visibilityGrid[tile.y * w + tile.x];
	}


	private static bool PushAdjacentTiles(Vector2i tile)
	{
		bool pushedAny = false;
		for (int i = 0; i < 4; ++i)
		{
			Vector2i adjacentTile = tile;
			adjacentTile.x += (1 - i / 2) * ((i % 2) * 2 - 1); // get horizontal and vertical tile offsets
			adjacentTile.y += (i / 2) * ((i % 2) * 2 - 1);

			if (GetTile(adjacentTile) == VisibilityState.open && !sm_openTiles.ContainsKey(adjacentTile))
			{
				sm_openTiles.Add(adjacentTile, false);
				pushedAny = true;
			}
		}

		return pushedAny;
	}

	private static bool VisitTile(Vector2i tile)
	{
		if (tile.Distance(sm_playerPos) > 30) // If outside sight range, invisible
			return false;

		if (GetTile(tile) == VisibilityState.open)
		{
			LosResult result = LineOfSight.CheckLOS(sm_navAgent, sm_playerPos, tile, tile.Distance(sm_playerPos), false); // Cast ray to current tile

			sm_visibilityGrid[tile.y * w + tile.x] = VisibilityState.visible; // If tile is visible, mark it as such
			if (result.blocked)
				sm_visibilityGrid[tile.y * w + tile.x] = sm_navAgent.CanAccess(tile) ? VisibilityState.open : VisibilityState.blocked; // If we can't see the tile, only close the node if it's a wall

			if(GetTile(tile) != VisibilityState.open) // Remove only closed nodes from open tiles' list
				sm_openTiles.Remove(tile);

			if (!result.blocked)
				return PushAdjacentTiles(tile); // Add adjacent tiles to open tiles' list
		}

		return false;
	}

	public static void UpdateLos()
	{
		//int sightRange = 10;

		var player = CharManager.GetLocalPlayer();

		if (player == null)
			return;

		var mover = player.m_mover;
		var navAgent = mover.m_navAgent;
		var startTile = mover.m_gridPos;
		sm_navAgent = navAgent;
		sm_playerPos = startTile;
		
		if (GetTile(startTile) == VisibilityState.open) // If no previous information, start search from current location
			VisitTile(startTile);

		bool newTiles = true;
		while (newTiles) // Keep looping over open tiles until new ones haven't been added to list of open tiles
		{
			newTiles = false;
			var keys = new List<Vector2i>(sm_openTiles.Keys);
			for (int i = 0; i < keys.Count; ++i)
			{
				Vector2i currentTile = keys[i];
				newTiles = newTiles || VisitTile(currentTile); // Visit current tile, checking line of sight and adding adjacent tiles to open tile list
			}
		}

		List<Vector3> visibleTiles = new List<Vector3>();
		List<Vector3> blockedTiles = new List<Vector3>();
		for (int x = 0; x < w; ++x)
		{
			for (int y = 0; y < h; ++y) // Loop over all tiles, adding visible tiles to a single list for rendering
			{
				Vector2i currentTile = new Vector2i(x, y);
				VisibilityState tileState = GetTile(currentTile);
				
				if (tileState == VisibilityState.visible)
					visibleTiles.Add(MapGrid.GridToWorldPoint(currentTile));
			}
		}

		var openkeys = new List<Vector2i>(sm_openTiles.Keys);
		for (int i = 0; i < openkeys.Count; ++i) // Loop over open nodes, create another list for visualization
		{
			Vector2i currentTile = openkeys[i];
			blockedTiles.Add(MapGrid.GridToWorldPoint(currentTile));
		}

		sm_closedLosRenderer.CreateMesh(blockedTiles.ToArray()); // Create meshes from tile lists
		sm_openLosRenderer.CreateMesh(visibleTiles.ToArray());
	}

	public static List<Vector3> BreadthFirstSearch() // Find any accessible nodes that can be moved into
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
