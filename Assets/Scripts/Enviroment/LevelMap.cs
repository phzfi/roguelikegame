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

        //PlaceRooms();
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
        rooms.Sort();
        rooms[0].isMain = true;
        rooms[0].isAccessibleFromMain = true;
        ConnectClosestRooms(rooms);
    }

    private void ConnectClosestRooms(List<Room> rooms, bool forceAccessiblityFromMain = false)
    {
        List<Room> firstRoomList = new List<Room>();
        List<Room> secondRoomList = new List<Room>();

        if (forceAccessiblityFromMain)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].isAccessibleFromMain)
                {
                    secondRoomList.Add(rooms[i]);
                } else
                {
                    firstRoomList.Add(rooms[i]);
                }
            }
        } else
        {
            firstRoomList = rooms;
            secondRoomList = rooms;
        }

        int shortestDistance = 0;
        Vector2i bestTileA = new Vector2i();
        Vector2i bestTileB = new Vector2i();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnection = false;

        for (int i = 0; i < firstRoomList.Count; i++)
        {
            if (!forceAccessiblityFromMain)
            {
                possibleConnection = false;
                if (firstRoomList[i].connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            for (int j = 0; j < secondRoomList.Count; j++)
            {
                if (firstRoomList[i] == secondRoomList[j] || firstRoomList[i].IsConnected(secondRoomList[j])) continue;
                
                for (int k = 0; k < firstRoomList[i].edgeTiles.Count; k++)
                {
                    for (int l = 0; l < secondRoomList[j].edgeTiles.Count; l++)
                    {
                        Vector2i tileA = firstRoomList[i].edgeTiles[k];
                        Vector2i tileB = secondRoomList[j].edgeTiles[l];
                        int distance = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2));

                        if (distance < shortestDistance || !possibleConnection)
                        {
                            shortestDistance = distance;
                            possibleConnection = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = firstRoomList[i];
                            bestRoomB = secondRoomList[j];
                        }
                    }
                }
            }
            if (possibleConnection && !forceAccessiblityFromMain)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnection && forceAccessiblityFromMain)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(rooms, true);
        }

        if (!forceAccessiblityFromMain)
        {
            ConnectClosestRooms(rooms, true);
        }
    }

    private void CreatePassage(Room roomA, Room roomB, Vector2i tileA, Vector2i tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        Debug.DrawLine(GridToWorldPos(tileA), GridToWorldPos(tileB), Color.green, 100.0f);

        List<Vector2i> line = GetLine(tileA, tileB);
        for (int i = 0; i < line.Count; i++)
        {
            DrawCircle(line[i], 2);
        }
    }

    private void DrawCircle(Vector2i c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.x + x;
                    int drawY = c.y + y;
                    if (IsInRange(drawX, drawY) && drawY != 0 && drawX != 0 && drawX != Width - 1 && drawY != Height - 1)
                    {
                        m_map[drawX, drawY].m_tileType = MapTileType.Floor;
                    }
                }
            }
        }
    }

    private List<Vector2i> GetLine(Vector2i from, Vector2i to)
    {
        List<Vector2i> line = new List<Vector2i>();
        int x = from.x;
        int y = from.y;

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        bool inverted = false;

        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);
        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Vector2i(x, y));

            if (inverted)
            {
                y += step;
            } else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                } else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
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
    
    class Room : IComparable<Room>
    {
        public List<Vector2i> tiles;
        public List<Vector2i> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMain;
        public bool isMain;
        
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

        public void SetAccessibleFromMain()
        {
            if (!isAccessibleFromMain)
            {
                isAccessibleFromMain = true;
                for (int i = 0; i < connectedRooms.Count; i++)
                {
                    connectedRooms[i].SetAccessibleFromMain();
                }
            }
        }

        public static void ConnectRooms(Room first, Room second)
        {
            if (first.isAccessibleFromMain)
            {
                second.SetAccessibleFromMain();
            } else if (second.isAccessibleFromMain)
            {
                first.SetAccessibleFromMain();
            }
            first.connectedRooms.Add(second);
            second.connectedRooms.Add(first);
        }

        public bool IsConnected(Room other)
        {
            return connectedRooms.Contains(other);
        }

        public int CompareTo(Room other)
        {
            return other.roomSize.CompareTo(roomSize);
        }

    }


}
