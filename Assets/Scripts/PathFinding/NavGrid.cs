using UnityEngine;
using System.Collections;

/*
 * World position is origo in case of the cell being unaccessible,
 * always check accessibility first!
*/
public class NavGridCell
{
    public NavGridCell() { m_worldPos = Vector3.zero; m_accessible = false; m_outOfTheScene = true; m_movementCost = 1.0f; m_smallestMaxAccessDistance = .0f; }

    public float m_smallestMaxAccessDistance;
    public Vector3 m_worldPos;
    public bool m_accessible;
    public bool m_outOfTheScene;
    public float m_movementCost;
};

public class NavGrid {

    public NavGridCell[,] m_navigationGrid;
    private int m_width = 1;
    public int Width { get { return m_width; } }
    private int m_height = 1;
    public int Height { get { return m_height; } }

    public NavGrid(int w, int h)
    {
        m_width = w;
        m_height = h;
        m_navigationGrid = new NavGridCell[w, h];
    }
}
