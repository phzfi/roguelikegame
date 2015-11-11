using UnityEngine;
using System.Collections;
using System;

public class NavGridScript : MonoBehaviour
{

    protected NavGridScript() { }

    public float m_width;
    public float m_height;
    public float m_cellSize = 1.0f;
    public float m_spiralSize = 20;
    public NavGrid m_grid;

    public int m_currentWidth { get; private set; }
    public int m_currentHeight { get; private set; }
    public float m_currentCellSize { get; private set; }
    private Vector2 m_BottomCornerWorldPosition;
    private LayerMask m_AccessibleLayer;
    private LayerMask m_invertAccessibleLayer;
    private LayerMask m_invertEnviromentMask;

    public LayerMask AccessibleMask
    {
        get { return m_AccessibleLayer;  }
    }

    public LayerMask invertEnviromentMask
    {
        get { return m_invertEnviromentMask; }
    }

    public LayerMask invertAccesbileEnviromentMask
    {
        get { return m_invertAccessibleLayer; }
    }

    void Awake()
    {
        GenerateNavGrid();
    }

    void OnDisable()
    {
    }

    public void GenerateNavGrid()
    {
        UpdateEnviromentMasks();
        m_currentCellSize = m_cellSize;
        m_currentWidth = Mathf.CeilToInt(m_width / m_cellSize) * 2 + 1;
        m_currentHeight = Mathf.CeilToInt(m_height / m_cellSize) * 2 + 1;
        var tom_leftm_bottomm_x = m_width + .5f * m_cellSize;
        var tom_leftm_bottomm_y = m_height + .5f * m_cellSize;
        m_BottomCornerWorldPosition = new Vector2(tom_leftm_bottomm_x, tom_leftm_bottomm_y) * -1.0f;
        //m_grid = new NavGrid(m_currentWidth, m_currentHeight);
        //RunRayCastingLoop(ref m_grid, Camera.main.transform.position.z);
        //RunPostProcessSpiral(ref m_grid);

        // Generointi tapahtuu LevelMapManagerissa
    }

    public void GenerateFromMap(LevelMap map)
    {
        //m_width = map.Width;
        //m_height = map.Height;
        //m_currentWidth = Mathf.CeilToInt(m_width / m_cellSize) * 2 + 1;
        //m_currentHeight = Mathf.CeilToInt(m_height / m_cellSize) * 2 + 1;
        NavGrid g = new NavGrid(m_currentWidth, m_currentHeight);

        // Vika on varmaan tässä loopissa tai currentPositionissa
        for (int x = 0; x < m_currentWidth; x++)
        {
            for (int y = 0; y < m_currentHeight; y++)
            {
                Vector3 currentPosition = new Vector3(-1.0f * m_width / 2 + x * m_currentCellSize, -1.0f * m_height / 2 + y * m_currentCellSize, 0);
                NavGridCell cell = new NavGridCell();
                if (x < map.Height && y < map.Width) {
                    cell.m_outOfTheScene = false;
                    if (map.GetTileType(x, y) == MapTile.Wall)
                    {
                        cell.m_worldPos = currentPosition;
                        cell.m_accessible = false;
                    }
                    else if (map.GetTileType(x, y) == MapTile.Floor)
                    {
                        cell.m_worldPos = currentPosition;
                        cell.m_accessible = true;
                    }
                } else
                {
                    cell.m_outOfTheScene = true;
                    cell.m_worldPos = currentPosition;
                    cell.m_accessible = false;
                }
                
                g.m_navigationGrid[x, y] = cell;
                m_grid = g;
            }
        }
        RunPostProcessSpiral(ref m_grid);
    }

    public void RunRayCastingLoop(ref NavGrid g, float d)
    {
        for (int x = 0; x < m_currentWidth; x++)
        {
            for (int y = 0; y < m_currentHeight; y++)
            {
                Vector3 currentPosition = new Vector3(-1.0f * m_width + x * m_currentCellSize, -1.0f * m_height + y * m_currentCellSize, d);
                RaycastHit hit;
                NavGridCell cell = new NavGridCell();
                if (Physics.Raycast(currentPosition, Vector3.forward, out hit, 100.0f, m_invertEnviromentMask))
                {
                    cell.m_worldPos = hit.point;
                    cell.m_outOfTheScene = false;
                    if (hit.collider.gameObject.layer == m_AccessibleLayer)
                        cell.m_accessible = true;
                }
                g.m_navigationGrid[x, y] = cell;
            }
        } 
    }

    public void RunPostProcessSpiral(ref NavGrid g)
    {
        for (int x = 0; x < m_currentWidth; ++x)
        {
            for (int y = 0; y < m_currentHeight; ++y)
            {
                NavGridCell cell = g.m_navigationGrid[x, y];
                if (!cell.m_accessible)
                    continue;
                Spiral s = new Spiral();
                bool hit = false;
                for (int i = 0; i < m_spiralSize; i++)
                {
                    int nextX, nextY;
                    s.Next(out nextX, out nextY);
                    if (x + nextX < 0 || x + nextX < 0 || y + nextY >= m_height || x + nextX >= m_width) // prevent indexing out of bounds
                        continue;
                    NavGridCell n = g.m_navigationGrid[(x + nextX), (y + nextY)];
                    if (!n.m_accessible)
                    {
                        Vector3 d = n.m_worldPos - cell.m_worldPos;
                        d.z = 0;
                        cell.m_smallestMaxAccessDistance = d.magnitude;
                        hit = true;
                        break;
                    }
                }
                if (!hit)
                {
                    int nextX, nextY;
                    s.Next(out nextX, out nextY);
                    cell.m_smallestMaxAccessDistance = (new Vector2(nextX, nextY)).magnitude;
                }
            }
        }
    }

    public bool IsGridGenerated(out int w, out int h)
    {
        w = m_currentWidth/2;
        h = m_currentHeight/2;
        return m_grid != null;
    }

    public void DeleteNavMesh() { m_grid = null; GC.Collect(); }

    void OnDrawGizmosSelected()
    {
        if (m_grid == null)
            return;

        for (var x = 0; x < m_currentWidth; x++)
        {
            for (var y = 0; y < m_currentHeight; y++)
            {
                NavGridCell cell = m_grid.m_navigationGrid[x, y];
                if (cell.m_outOfTheScene)
                    continue;
                Gizmos.color = cell.m_accessible ? Color.green : Color.red;
                Vector3 drawPosition = cell.m_worldPos + Vector3.back * .25f;
                Vector3 s = Vector3.one * m_currentCellSize * 0.7f;
                if(cell.m_accessible)
                    s.z = cell.m_smallestMaxAccessDistance;
                Gizmos.DrawCube(drawPosition, s);
            }
        }
    }

    public void ClearPatrolRoutes()
    {
        int c = transform.childCount;
        for (int i = c; i > 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i-1).gameObject);
        }
    }

    public bool IsWorldPositionAccessable(ref float x, ref float y)
    {
        var xm_fromm_bottomm_ofm_grid = (x + m_width) / m_cellSize + .5f;
        var ym_fromm_bottomm_ofm_grid = (y + m_height) / m_cellSize + .5f;
        var index_x = Mathf.FloorToInt(xm_fromm_bottomm_ofm_grid);
        var index_y = Mathf.FloorToInt(ym_fromm_bottomm_ofm_grid);
        x = m_BottomCornerWorldPosition.x + ((float)index_x + .5f) * m_cellSize;
        y = m_BottomCornerWorldPosition.y + ((float)index_y + .5f) * m_cellSize;

        if (index_x < 0 || index_x >= m_grid.m_navigationGrid.GetLength(0))
            return false;
        else if (index_y < 0 || index_y >= m_grid.m_navigationGrid.GetLength(1))
            return false;
        else
            return m_grid.m_navigationGrid[index_x, index_y].m_accessible; 
    }


    public Vector2i GetGridPosition(Vector3 worldPosition)
    {
        return new Vector2i
        {
            x = Mathf.FloorToInt((worldPosition.x + ((float)m_currentWidth * m_currentCellSize) * .5f) / m_currentCellSize),
            y = Mathf.FloorToInt((worldPosition.y + ((float)m_currentHeight * m_currentCellSize) * .5f) / m_currentCellSize)
        };
    }

    public Vector3 GetWorldPos(Vector2i gridPos)
    {
        return m_grid.m_navigationGrid[gridPos.x, gridPos.y].m_worldPos;
    }

    private void UpdateEnviromentMasks()
    {
        m_AccessibleLayer = LayerMask.NameToLayer("EnviromentAccessible");
        m_invertAccessibleLayer = 1 << m_AccessibleLayer;
        m_invertEnviromentMask = m_invertAccessibleLayer | 1 << LayerMask.NameToLayer("EnviromentUnaccessible");
    }
}
