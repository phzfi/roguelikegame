using UnityEngine;
using System.Collections;
using System;

public enum MapTileType
{
	Empty = 0,
	Wall = 1,
	Floor = 2,
	Count = 3
}

public struct MapTile
{
	public MapTileType m_tileType;
	public int m_visualizationIndex;
}

public class LevelMap : MonoBehaviour
{
	public bool m_useRandomSeed;
	public string m_seed = System.DateTime.Now.ToString();
	[Range(0, 100)]
	public int randomFillPercent;

	private Vector2i m_size = new Vector2i(0, 0);
	private MapTile[,] m_map;
	private NavGrid m_navGrid = null;

	public int Width { get { return m_size.x; } }
	public int Height { get { return m_size.y; } }
	public Vector2i Size { get { return m_size; } }

	public MapTileType GetTileType(int x, int y)
	{
		Debug.Assert(m_map != null, "Trying to access map data before its created!");
		return m_map[x, y].m_tileType;
	}

	public NavGrid GetNavGrid()
	{
		Debug.Assert(m_navGrid != null, "Trying to access map nav data before its created!");
		return m_navGrid;
	}

	public void Generate(int width, int height)
	{
		m_size.x = width;
		m_size.y = height;
		m_map = new MapTile[m_size.x, m_size.y];

		RandomFillMap();

		for (int i = 0; i < 4; i++)
			SmoothMap();

		m_navGrid = new NavGrid(this);
	}

	private void RandomFillMap()
	{
		if (m_useRandomSeed)
		{
			m_seed = System.DateTime.Now.ToString();
		}
		System.Random pseudoRandom = new System.Random(m_seed.GetHashCode());

		for (int x = 0; x < m_size.x; x++)
		{
			for (int y = 0; y < m_size.y; y++)
			{
				MapTile mapItem = new MapTile();

				if (x == 0 || x == m_size.x - 1 || y == 0 || y == m_size.y - 1)
				{
					mapItem.m_tileType = MapTileType.Wall;
				}
				else
				{
					mapItem.m_tileType = (pseudoRandom.Next(0, 100) < randomFillPercent) ? MapTileType.Wall : MapTileType.Floor;
				}
				m_map[x, y] = mapItem;
			}
		}
	}

	private void SmoothMap()
	{
		for (int x = 0; x < m_size.x; x++)
		{
			for (int y = 0; y < m_size.y; y++)
			{
				int neighbourWallTiles = GetSurroundingWallCount(x, y);

				if (neighbourWallTiles > 4)
				{
					MapTile mapItem = new MapTile();
					mapItem.m_tileType = MapTileType.Wall;
					m_map[x, y] = mapItem;
				}
				else if (neighbourWallTiles < 4)
				{
					MapTile mapItem = new MapTile();
					mapItem.m_tileType = MapTileType.Floor;
					m_map[x, y] = mapItem;
				}

			}
		}
	}

	private int GetSurroundingWallCount(int gridX, int gridY)
	{
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
			{
				if (neighbourX >= 0 && neighbourX < m_size.x && neighbourY >= 0 && neighbourY < m_size.y)
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						wallCount += (m_map[neighbourX, neighbourY].m_tileType == MapTileType.Wall) ? 1 : 0;
					}
				}
				else
				{
					wallCount++;
				}
			}
		}
		return wallCount;
	}
}
