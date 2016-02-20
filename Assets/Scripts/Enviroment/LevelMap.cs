using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public enum MapTileType
{
	Empty = 0,
	Wall = 1,
	Floor = 2,
	Count = 3
}

[Serializable]
public struct MapTile
{
    public MapTileType m_tileType;
	public int m_visualizationIndex;
	public bool m_isAccessible;
}

public struct MapRandomizationData
{
    public bool useRandomSeed;
    public int width;
    public int height;

    [Range(0, 100)]
    public int randomFillPercent;
    public int smoothingIterations;

    public float frequency;
    public int roomThresholdSize;
    public int passageWidth;

    [Range(0, 25)]
    public int spaceCount;
    public int maxSpaceSize;
    public int minSpaceSize;
    public int meanSpaceSize;
    public float standardDeviation;
}

public class LevelMap
{
    public LevelMap(bool isComplete)
    {
        m_isComplete = isComplete;
    }

    public bool m_useRandomSeed;
    public string m_seed = "0";

    [Range(0, 100)]
	private int m_randomFillPercent;
    private int m_smoothingIterations;
    
    private float m_frequency;
    private int m_roomThresholdSize;
    private int m_passageWidth;

    [Range(0, 25)]
    private int m_spaceCount;
    private int m_maxSpaceSize;
    private int m_minSpaceSize;
    private int m_meanSpaceSize;
    private float m_standardDeviation;

    private bool m_isComplete;
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

    public int GetVisualizationIndex(int x, int y)
    {
        Debug.Assert(m_map != null, "Trying to access map data before its created!");
        return m_map[x, y].m_visualizationIndex;
    }

    public bool IsAccessible(int x, int y) {
		Debug.Assert(m_map != null, "Trying to access map data before its created!");
		return m_map [x, y].m_isAccessible;
	}

	public NavGrid GetNavGrid()
	{
		Debug.Assert(m_navGrid != null, "Trying to access map nav data before its created!");
		return m_navGrid;
	}

	public void Generate(GameLogic data)
	{
        m_useRandomSeed = data.m_useRandomSeed;
        m_size = new Vector2i(data.m_width, data.m_height);
        m_randomFillPercent = data.m_randomFillPercent;
        m_smoothingIterations = data.m_smoothingIterations;
        m_frequency = data.m_frequency;
        m_roomThresholdSize = data.m_roomThresholdSize;
        m_passageWidth = data.m_passageWidth;

        m_spaceCount = data.m_spaceCount;
        m_maxSpaceSize = data.m_maxSpaceSize;
        m_minSpaceSize = data.m_minSpaceSize;
        m_meanSpaceSize = data.m_meanSpaceSize;
        m_standardDeviation = data.m_standardDeviation;

		m_map = new MapTile[m_size.x, m_size.y];

        if (m_useRandomSeed) m_seed = System.DateTime.Now.ToString();

        //RandomFillMap();
        PerlinNoiseMap();

		for (int i = 0; i < m_smoothingIterations; i++)
			SmoothMap();

		if (m_spaceCount > 0)
			makeEmptySpaces();

		ProcessMap();
		
        RemoveThinWalls();

		UpdateAccessibility ();

		m_navGrid = new NavGrid(this, true);
        GenerateVisualizationIndices();
    }

    void GenerateVisualizationIndices()
    {
        for (int x = 0; x < m_size.x; ++x)
        {
            for (int y = 0; y < m_size.y; ++y)
                m_map[x, y].m_visualizationIndex = LevelMapUtilities.GetMeshVisualizationIndex(this, x, y);
        }
    }

    public void GenerateEmpty(int width, int height)
    {
        m_size.x = width;
        m_size.y = height;
        m_map = new MapTile[m_size.x, m_size.y];
        m_navGrid = new NavGrid(m_size);
    }

    public void AddToMap(Vector2i p, MapTileType type, int visualizationIndex)
    {
        m_map[p.x, p.y].m_tileType = type;
        m_map[p.x, p.y].m_visualizationIndex = visualizationIndex;
        m_map[p.x, p.y].m_isAccessible = type == MapTileType.Floor;
    }

    public void GetAllVisibleTiles(Vector2i p, ref List<Vector2i> positions, 
        ref List<int> tiles, ref List<int> visualizationIndices)
    {
        for(int x = 0; x < m_size.x; ++x)
        {
            for (int y = 0; y < m_size.y; ++y)
            {
                Vector2i position;
                position.x = x;
                position.y = y;
                positions.Add(position);
                tiles.Add((int)m_map[x, y].m_tileType);
                visualizationIndices.Add(m_map[x,y].m_visualizationIndex);
            }
        }
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

	private void UpdateAccessibility()
	{
		for (int x = 0; x < m_size.x; x++)
		{
			for (int y = 0; y < m_size.y; y++)
			{
				if (x == 0 || y == 0 || y == m_size.y - 1 || x == m_size.x - 1)
				{
					m_map[x, y].m_isAccessible = false;
				} else
				{
					int neighbor_walls = 0;
					if (GetTileType(x, y) == MapTileType.Wall)
						neighbor_walls += 1;
					if (GetTileType (x + 1, y) == MapTileType.Wall)
						neighbor_walls += 1;
					if (GetTileType (x, y + 1) == MapTileType.Wall)
						neighbor_walls += 1;
					if (GetTileType(x + 1, y + 1) == MapTileType.Wall)
						neighbor_walls += 1;

					if (neighbor_walls >= 2)
						m_map[x, y].m_isAccessible = false;
					else
						m_map[x, y].m_isAccessible = true;
				}
			}
		}
	}

    private void makeEmptySpaces()
    {
        // Create empty rectangular spaces around on map
        System.Random pseudoRandom = new System.Random(m_seed.GetHashCode());

        for (int i = 0; i < m_spaceCount; i++)
        {
            int spaceWidth = (int)NextGaussian(m_meanSpaceSize, m_standardDeviation, m_minSpaceSize, m_maxSpaceSize, pseudoRandom);
            int spaceHeight = (int)NextGaussian(m_meanSpaceSize, m_standardDeviation, m_minSpaceSize, m_maxSpaceSize, pseudoRandom);
            int x = pseudoRandom.Next(0, m_size.x - spaceWidth - 1);
            int y = pseudoRandom.Next(0, m_size.y - spaceHeight - 1);
            for (int j = x; j < x + spaceWidth; j++)
            {
                for (int k = y; k < y + spaceHeight; k++)
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

        List<Room> rooms = new List<Room>();

        for (int i = 0; i < roomRegions.Count; i++)
        {
            if (roomRegions[i].Count < m_roomThresholdSize)
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

        // Sort from largest room to smallest
        // We pick our largest room as our "Main" room which we will check connectivity to.
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
                    // These are connected to the main room
                    secondRoomList.Add(rooms[i]);
                } else
                {
                    // These are not connected to the main room
                    firstRoomList.Add(rooms[i]);
                }
            }
        } else
        {
            // If we do not care about accessibility from main
            // we can just keep these two the same.
            firstRoomList = rooms;
            secondRoomList = rooms;
        }

        int shortestDistance = 0;
        Vector2i bestTileA = new Vector2i();
        Vector2i bestTileB = new Vector2i();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnection = false;

        // Find the shortest possible connections
        // For forced accessibility from main we need to look at all possible rooms
        // that are not connected to main.
        // Not forced, means we run it independently for each room and connect them to their nearest neighbor.
        for (int i = 0; i < firstRoomList.Count; i++)
        {
            if (!forceAccessiblityFromMain)
            {
                possibleConnection = false;
                // If we do not care about accessibility from main room, one connection is enough.
                if (firstRoomList[i].connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            for (int j = 0; j < secondRoomList.Count; j++)
            {
                // Skip if the rooms are equal or already connected to each other.
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
            // One good connection for each room is enough if forcing is not enabled.
            if (possibleConnection && !forceAccessiblityFromMain)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        // We have checked through all possible rooms for a good connection.
        if (possibleConnection && forceAccessiblityFromMain)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(rooms, true);
        }

        // First we ran one iteration to connect all nearby rooms their nearest neighbor
        // Then we need to force that every room is accessible from the main room 
        if (!forceAccessiblityFromMain)
        {
            ConnectClosestRooms(rooms, true);
        }
    }

    private void CreatePassage(Room roomA, Room roomB, Vector2i tileA, Vector2i tileB)
    {
        // Creates a passage between two rooms from tileA to tileB
        Room.ConnectRooms(roomA, roomB);
        List<Vector2i> line = GetLine(tileA, tileB);
        for (int i = 0; i < line.Count; i++)
        {
            DrawCircle(line[i], m_passageWidth);
        }
    }

    private void DrawCircle(Vector2i c, int r)
    {
        // Turns tiles inside the circle with center c and radius r into floor tiles
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
        // Bresenham's line algorithm, returns the map tile positions along a line.
        List<Vector2i> line = new List<Vector2i>();
        int x = from.x;
        int y = from.y;

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        bool inverted = false;
        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);
        int step; int gradientStep;

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        } else
        {
            step = Math.Sign(dx);
            gradientStep = Math.Sign(dy);
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

    private List<List<Vector2i>> GetRegions(MapTileType type)
    {
        // Get regions of a certain tile type.
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
        // Flood fill algorithm, fills in neighbors until it collides with a wall
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

            // Check neighboring tiles if they haven't been visited and visit and put into queue if not visited.
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

            // Create edge tiles
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
            // Recursively set accessibility for this room's connections.
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
            // If one is accessible from main, we need to set the other to be accessible.
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

