using UnityEngine;
using System.Collections;
using System;

public enum MapTile
{
    Empty = 0,
    Wall = 1,
    Floor = 2,
    Count = 3
}

/*
For now holds only type of the map and the index of to certain visualization model for that tyle.
*/

    public struct LevelMapItem
{
    public MapTile m_tile;
    public int m_visualization_index;
}

public class LevelMap : MonoBehaviour
{
    private int m_width = 1;
    public int Width { get { return m_width; } }
    private int m_height = 1;
    public int Height { get { return m_height; } }
    public bool m_useRandomSeed;
    public string m_seed = System.DateTime.Now.ToString();
    [Range(0, 100)]
    public int randomFillPercent;
    
    private LevelMapItem[,] m_map;

    public void Generate()
    {
        LevelMapManager manager = GetComponent<LevelMapManager>();
        m_width = manager.m_width;
        m_height = manager.m_height;
        m_map = new LevelMapItem[m_width, m_height];

        RandomFillMap();

        for (int i = 0; i < 4; i++)
            SmoothMap();
    }

    void RandomFillMap()
    {
        if (m_useRandomSeed)
        {
            m_seed = System.DateTime.Now.ToString();
        }
        System.Random pseudoRandom = new System.Random(m_seed.GetHashCode());

        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                LevelMapItem mapItem = new LevelMapItem();

                if (x == 0 || x == m_width - 1 || y == 0 || y == m_height - 1)
                {
                    mapItem.m_tile = MapTile.Wall;
                } else
                {
                    mapItem.m_tile = (pseudoRandom.Next(0, 100) < randomFillPercent) ? MapTile.Wall : MapTile.Floor;
                }
                m_map[x, y] = mapItem;
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                {
                    LevelMapItem mapItem = new LevelMapItem();
                    mapItem.m_tile = MapTile.Wall;
                    m_map[x, y] = mapItem;
                }
                else if (neighbourWallTiles < 4)
                {
                    LevelMapItem mapItem = new LevelMapItem();
                    mapItem.m_tile = MapTile.Floor;
                    m_map[x, y] = mapItem;
                }
                
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < m_width && neighbourY >= 0 && neighbourY < m_height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += (m_map[neighbourX, neighbourY].m_tile == MapTile.Wall) ? 1 : 0;
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

    // For precalculated NavGrid used for development
    public void Generate(NavGrid precalculated_grid)
    {
        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; y++)
            {
                LevelMapItem mapItem = new LevelMapItem();
                NavGridCell navCell = precalculated_grid.m_navigationGrid[x, y];
                if (navCell.m_outOfTheScene)
                    mapItem.m_tile = MapTile.Empty;
                else if (navCell.m_accessible)
                    mapItem.m_tile = MapTile.Floor;
                else
                    mapItem.m_tile = MapTile.Wall;

                m_map[x, y] = mapItem;
            }
        }
    }

    public MapTile GetTileType(int x, int y)
    {
        return m_map[x, y].m_tile;
    }

    
}
