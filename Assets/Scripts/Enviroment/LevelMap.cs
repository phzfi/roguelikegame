using UnityEngine;
using System.Collections;

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

    private LevelMapItem[,] m_map;

    private LevelMap() { }
    public LevelMap(int w, int h)
    {
        m_width = w;
        m_height = h;
        m_map = new LevelMapItem[w, h];
    }

    public void Generate()
    {
        //TODO
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
