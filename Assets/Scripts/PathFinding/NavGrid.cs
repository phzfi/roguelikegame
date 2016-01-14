using UnityEngine;
using System.Collections;

public class NavGridCell
{
	public bool m_accessible;
	public float m_movementCost;
	public float m_maxAgentRadius;

	public NavGridCell()
	{
		m_accessible = false;
		m_movementCost = 1.0f;
		m_maxAgentRadius = 0.0f;
	}
};

public class NavGrid
{
	public const float m_spiralSize = 30.0f; // TODO correct place for this?

	public NavGridCell[,] m_navigationGrid;

	private Vector2i m_size = new Vector2i();

	public int Width { get { return m_size.x; } }
	public int Height { get { return m_size.y; } }
	public Vector2i Size { get { return m_size; } }

	public NavGrid(LevelMap map)
	{
		m_size = map.Size;
		m_navigationGrid = new NavGridCell[m_size.x, m_size.y];
		GenerateFromMap(map);
	}

	public bool IsAccessible(Vector2i gridPos)
	{
		return IsAccessible(gridPos.x, gridPos.y);
	}

	public bool IsAccessible(int gridX, int gridY)
	{
		if (gridX < 0 || gridY < 0)
			return false;
		if (gridX >= m_size.x || gridY >= m_size.x)
			return false;
		return m_navigationGrid[gridX, gridY].m_accessible;
	}

	public bool IsAccessibleForSize(Vector2i gridPos, float agentRadius)
	{
		return IsAccessibleForSize(gridPos.x, gridPos.y, agentRadius);
	}

	public bool IsAccessibleForSize(int gridX, int gridY, float agentRadius)
	{
		if (!IsAccessible(gridX, gridY))
			return false;

		return agentRadius < m_navigationGrid[gridX, gridY].m_maxAgentRadius;
	}

	public Vector2i FindClosestAccessiblePosition(Vector2i gridPos, float agentRadius)
	{
		if (IsAccessibleForSize(gridPos, agentRadius))
		{
			return gridPos;
		}

		Spiral spiral = new Spiral();
		for (int i = 0; i < NavGrid.m_spiralSize; i++)
		{
			Vector2i offset = spiral.GetNextOffset();

			if (IsAccessibleForSize(gridPos + offset, agentRadius))
			{
				return gridPos + offset;
			}
		}

		Debug.LogError("Failed to find an accessible position");
		return gridPos;
	}

	public void GenerateFromMap(LevelMap map)
	{
		for (int x = 0; x < m_size.x; x++)
		{
			for (int y = 0; y < m_size.y; y++)
			{
				NavGridCell cell = new NavGridCell();
				MapTileType tileType = map.GetTileType(x, y);
				switch (tileType)
				{
					case MapTileType.Empty:
					case MapTileType.Wall:
						cell.m_accessible = false;
						break;
					case MapTileType.Floor:
						cell.m_accessible = true;
						break;
					default:
						Debug.Assert(false, "Unknown map tile type: " + tileType);
						break;
				}
				m_navigationGrid[x, y] = cell;
			}
		}

		RunPostProcessSpiral();
	}

	private void RunPostProcessSpiral()
	{
		for (int x = 0; x < m_size.x; ++x)
		{
			for (int y = 0; y < m_size.y; ++y)
			{
				NavGridCell cell = m_navigationGrid[x, y];
				if (!cell.m_accessible)
					continue;

				Spiral spiral = new Spiral();
				bool hit = false;

				for (int i = 0; i < m_spiralSize; i++)
				{
					Vector2i offset = spiral.GetNextOffset();
					if (!IsAccessible(x + offset.x, y + offset.y))
					{
						cell.m_maxAgentRadius = offset.Length();
						hit = true;
						break;
					}
				}

				if (!hit)
				{
					Vector2i offset = spiral.GetNextOffset();
					cell.m_maxAgentRadius = offset.Length();
				}
			}
		}
	}
}
