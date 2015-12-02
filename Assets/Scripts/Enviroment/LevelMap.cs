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
	public int m_randomFillPercent;
    public int m_smoothingIterations;

    public float m_frequency;
    public int m_roomCount;
    public int m_maxRoomSize;
    public int m_minRoomSize;
    public int m_meanRoomSize;
    public float m_standardDeviation;

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

        if (m_useRandomSeed) m_seed = System.DateTime.Now.ToString();

        //RandomFillMap();
        PerlinNoiseMap();

		for (int i = 0; i < m_smoothingIterations; i++)
			SmoothMap();

        PlaceRooms();
        RemoveThinWalls();

		m_navGrid = new NavGrid(this);
	}

	private void RandomFillMap()
	{
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
					mapItem.m_tileType = (pseudoRandom.Next(0, 100) < m_randomFillPercent) ? MapTileType.Wall : MapTileType.Floor;
				}
				m_map[x, y] = mapItem;
			}
		}
	}

    private void PerlinNoiseMap()
    {
        System.Random pseudoRandom = new System.Random(m_seed.GetHashCode());
        float xOffset = pseudoRandom.Next(-1000, 1000);
        float yOffset = pseudoRandom.Next(-1000, 1000);

        for (int x = 0; x < m_size.x; x++)
        {
            for (int y = 0; y < m_size.y; y++)
            {
                MapTile mapItem = new MapTile();
                if (x == 0 || x == m_size.x - 1 || y == 0 || y == m_size.y - 1)
                    mapItem.m_tileType = MapTileType.Wall;
                else
                {
                    float xVal = x * m_frequency + xOffset;
                    float yVal = y * m_frequency + yOffset;
                    float noiseValue = Mathf.PerlinNoise(xVal, yVal);
                    mapItem.m_tileType = (noiseValue * 100 < m_randomFillPercent) ? MapTileType.Wall : MapTileType.Floor;
                }
                m_map[x, y] = mapItem;
            }
        }
    }

    private void PlaceRooms()
    {
        System.Random pseudoRandom = new System.Random(m_seed.GetHashCode());

        for (int i = 0; i < m_roomCount; i++)
        {
            int roomWidth = (int)NextGaussian(m_meanRoomSize, m_standardDeviation, m_minRoomSize, m_maxRoomSize, pseudoRandom);
            int roomHeight = (int)NextGaussian(m_meanRoomSize, m_standardDeviation, m_minRoomSize, m_maxRoomSize, pseudoRandom);
            int x = pseudoRandom.Next(0, m_size.x - roomWidth - 1);
            int y = pseudoRandom.Next(0, m_size.y - roomHeight - 1);
            for (int j = x; j < x + roomWidth; j++)
            {
                for (int k = y; k < y + roomHeight; k++)
                {
                    if (j != 0 && k != 0 && k != m_size.y - 1 && j != m_size.x - 1)
                        m_map[j, k].m_tileType = MapTileType.Floor;
                }
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

    private void RemoveThinWalls()
    {
        for (int x = 1; x < m_size.x - 1; x++)
        {
            for (int y = 1; y < m_size.y - 1; y++)
            {
                if (m_map[x,y].m_tileType == MapTileType.Wall) {
                    if (m_map[x - 1, y].m_tileType == MapTileType.Floor && m_map[x + 1, y].m_tileType == MapTileType.Floor)
                        m_map[x, y].m_tileType = MapTileType.Floor;
                    else if (m_map[x, y - 1].m_tileType == MapTileType.Floor && m_map[x, y + 1].m_tileType == MapTileType.Floor)
                        m_map[x, y].m_tileType = MapTileType.Floor;
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

    private float NextGaussian(System.Random pseudoRandom)
    {
        float u, v, S;
        do
        {
            u = 2.0f * (float)pseudoRandom.NextDouble() - 1.0f;
            v = 2.0f * (float)(pseudoRandom.NextDouble()) - 1.0f;
            S = u * u + v * v;
        } while (S >= 1.0);
        return u * (float)Math.Sqrt(-2.0f * Math.Log(S) / S);
    }

    private float NextGaussian(float mean, float standardDeviation, float min, float max, System.Random pseudoRandom)
    {
        float val;
        do
        {
            val = mean + NextGaussian(pseudoRandom) * standardDeviation;
        } while (val < min || val > max);
        return val;
    }
}
