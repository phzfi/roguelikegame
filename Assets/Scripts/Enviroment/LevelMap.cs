using UnityEngine;
using System.Collections.Generic;
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

        ProcessMap();

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

    private void ProcessMap()
    {
        // Create rooms and discard tiny rooms
        List<List<Vector2i>> roomRegions = GetRegions(MapTileType.Floor);
        int roomThresholdSize = 30;

        List<Room> rooms = new List<Room>();

        for (int i = 0; i < roomRegions.Count; i++)
        {
            if (roomRegions[i].Count < roomThresholdSize)
            {
                for (int j = 0; j < roomRegions[i].Count; j++)
                {
                    m_map[roomRegions[i][j].x, roomRegions[i][j].y].m_tileType = MapTileType.Wall;
                }
            } else
            {
                rooms.Add(new Room(roomRegions[i], m_map));
            }
        }
        ConnectClosestRooms(rooms);
    }

    private void ConnectClosestRooms(List<Room> rooms)
    {
        int shortestDistance = 0;
        Vector2i bestTileA = new Vector2i();
        Vector2i bestTileB = new Vector2i();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnection = false;

        for (int i = 0; i < rooms.Count; i++)
        {
            possibleConnection = false;

            for (int j = 0; j < rooms.Count; j++)
            {
                if (rooms[i] == rooms[j]) continue;
                if (rooms[i].IsConnected(rooms[j]))
                {
                    possibleConnection = false;
                    break;
                }
                for (int k = 0; k < rooms[i].edgeTiles.Count; k++)
                {
                    for (int l = 0; l < rooms[j].edgeTiles.Count; l++)
                    {
                        Vector2i tileA = rooms[i].edgeTiles[k];
                        Vector2i tileB = rooms[j].edgeTiles[l];
                        int distance = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2));

                        if (distance < shortestDistance || !possibleConnection)
                        {
                            shortestDistance = distance;
                            possibleConnection = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = rooms[i];
                            bestRoomB = rooms[j];
                        }
                    }
                }
            }
            if (possibleConnection)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    private void CreatePassage(Room roomA, Room roomB, Vector2i tileA, Vector2i tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        Debug.DrawLine(GridToWorldPos(tileA), GridToWorldPos(tileB), Color.green, 100.0f);
    }

    // temporary debug function
    private Vector3 GridToWorldPos(Vector2i tile)
    {
        float x = ((float)tile.x + 0.5f) * 1.5f;
        float y = ((float)tile.y + 0.5f) * 1.5f;
        return new Vector3(x, y, 0.0f);
    }

    private List<List<Vector2i>> GetRegions(MapTileType type)
    {
        List<List<Vector2i>> regions = new List<List<Vector2i>>();
        bool[,] visited = new bool[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!visited[x, y] && m_map[x, y].m_tileType == type)
                {
                    List<Vector2i> region = GetRegionTiles(x, y);
                    regions.Add(region);

                    for (int i = 0; i < region.Count; i++)
                    {
                        visited[region[i].x, region[i].y] = true;
                    }

                }
            }
        }

        return regions;
    }

    private List<Vector2i> GetRegionTiles(int startX, int startY)
    {
        List<Vector2i> tiles = new List<Vector2i>();
        bool[,] visited = new bool[Width, Height];
        MapTileType type = m_map[startX, startY].m_tileType;
        Queue<Vector2i> queue = new Queue<Vector2i> ();
        queue.Enqueue(new Vector2i(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2i tile = queue.Dequeue();
            tiles.Add(tile);

            if (IsInRange(tile.x - 1, tile.y) && !visited[tile.x - 1, tile.y] && m_map[tile.x - 1, tile.y].m_tileType == type)
            {
                visited[tile.x - 1, tile.y] = true;
                queue.Enqueue(new Vector2i(tile.x - 1, tile.y));
            }

            if (IsInRange(tile.x + 1, tile.y) && !visited[tile.x + 1, tile.y] && m_map[tile.x + 1, tile.y].m_tileType == type)
            {
                visited[tile.x + 1, tile.y] = true;
                queue.Enqueue(new Vector2i(tile.x + 1, tile.y));
            }

            if (IsInRange(tile.x, tile.y - 1) && !visited[tile.x, tile.y - 1] && m_map[tile.x, tile.y - 1].m_tileType == type)
            {
                visited[tile.x, tile.y - 1] = true;
                queue.Enqueue(new Vector2i(tile.x, tile.y - 1));
            }

            if (IsInRange(tile.x, tile.y + 1) && !visited[tile.x, tile.y + 1] && m_map[tile.x, tile.y + 1].m_tileType == type)
            {
                visited[tile.x, tile.y + 1] = true;
                queue.Enqueue(new Vector2i(tile.x, tile.y + 1));
            }
        }
        return tiles;
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

    private bool IsInRange(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
	{
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
			{
				if (IsInRange(neighbourX, neighbourY))
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
    
    class Room
    {
        public List<Vector2i> tiles;
        public List<Vector2i> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        
        public Room() {}

        public Room(List<Vector2i> roomTiles, MapTile[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();
            edgeTiles = new List<Vector2i>();

            for (int i = 0; i < roomSize; i++)
            {
                Vector2i tile = tiles[i];
                for (int x = tile.x - 1; x <= tile.x + 1; x++)
                {
                    for (int y = tile.y - 1; y <= tile.y + 1; y++)
                    {
                        if ((x == tile.x || y == tile.y) && map[x, y].m_tileType == MapTileType.Wall)
                        {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }

        public static void ConnectRooms(Room first, Room second)
        {
            first.connectedRooms.Add(second);
            second.connectedRooms.Add(first);
        }

        public bool IsConnected(Room other)
        {
            return connectedRooms.Contains(other);
        }

    }


}
