using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct NavMovementEvalData
{
    public float f;
}

public delegate float MovementEvalFuncDelegate(NavMovementEvalData d);

public class NavPath {

    public float m_characterSize = .1f;
    public List<Vector3> m_path;
    public bool m_catmullRom = false;
    public Vector3 m_startWorldPos;
    public Vector3 m_endWorldPos;

    private NavGridScript m_data;
    private bool m_init = false;

    public class Node
    {
        public Node() { g_score = float.MaxValue; }
        public Node(float f, float g) { f_score = f; g_score = g; cameFrom = new Vector2i(); }
        public Node(float f, float g, Vector2i c) { f_score = f; g_score = g; cameFrom = c; }

        public float g_score;
        public float f_score;
        public Vector2i cameFrom;
    }


    public void Initialize(NavGridScript pathFindingData)
    {
        m_data = pathFindingData;
        m_init = true;
        m_path = new List<Vector3>();
    }

    public List<Vector3> SeekPath(MovementEvalFuncDelegate del, Vector3 startWorldPos, Vector3 endWorldPos)
    {
        List<Vector3> path = new List<Vector3>();
        if (!m_init)
        {
            Debug.LogError("Trying to use NavPath without initialization");
            return null;
        }

        path.Clear();
        m_catmullRom = false;
        m_startWorldPos = startWorldPos;
        Vector2i start = GetGridPosition(startWorldPos);
        Vector2i end = GetGridPosition(endWorldPos);
		if (start == end)
			return path;

        FindClosetAccessableWithSpiral(ref end);
        
        var closedSet = new Dictionary<Vector2i, Node>(); 
        var map = new Dictionary<Vector2i, Node>();
        var openSet = new Dictionary<Vector2i, Node>();
        var startNode = new Node(start.Distance(end), .0f, Vector2i.None);



        map[start] = startNode;
        openSet[start] = startNode;

        while (openSet.Count > 0)
        {
            var best = openSet.Aggregate((c, n) => c.Value.f_score < n.Value.f_score ? c : n);
            openSet.Remove(best.Key);
            closedSet[best.Key] = best.Value;
            if (best.Key == end)
            {
                if (path == null)
                    path = new List<Vector3>();
                else
                    path.Clear();

                Node n = best.Value;
                path.Add(GetWorldPos(best.Key));
                while (n != null && n.cameFrom != Vector2i.None)
                {
                    path.Insert(0, GetWorldPos(n.cameFrom));
                    n = map[n.cameFrom];
                }
                return path;
            }

            var neighbours = GetNeighbours(best.Key);
            for (int i = 0; i < neighbours.Count; ++i)
            {
                Vector2i n = neighbours[i];
                if (!IsAccessableForSize(n) || closedSet.ContainsKey(n))
                    continue;

                NavMovementEvalData d = new NavMovementEvalData();
                d.f = 1.0f;
                float g = best.Value.g_score + del(d);
                Node c;
                if (!openSet.TryGetValue(n, out c))
                {
                    c = new Node();
                    map[n] = c;
                    openSet[n] = c;
                }
                if (c.g_score > g)
                {
                    c.g_score = g;
                    c.cameFrom = best.Key;
                    c.f_score = g + n.Distance(end);
                }

            }
        }
        return path;
    }

    public Vector3 GetClosetAccessibleWorldPositionOnGrid(Vector3 p)
    {
        Vector2i gp = GetGridPosition(p);
        FindClosetAccessableWithSpiral(ref gp);
        return GetWorldPos(gp);
    }

    void FindClosetAccessableWithSpiral(ref Vector2i gp)
    {
        if (!IsAccessableForSize(gp))
        {
            Spiral s = new Spiral();
            for (int i = 0; i < m_data.m_spiralSize; i++)
            {
                int nextX, nextY;
                s.Next(out nextX, out nextY);
                Vector2i n = gp;
                n.x += nextX;
                n.y += nextY;
                if (IsAccessableForSize(n))
                {
                    gp = n;
                    break;
                }
            }
        } 
    }

    public Vector2i GetGridPosition(Vector3 worldPosition)
    {
        return new Vector2i
        {
            x = Mathf.FloorToInt((worldPosition.x + ((float)m_data.m_currentWidth * m_data.m_currentCellSize) * .5f) / m_data.m_currentCellSize),
            y = Mathf.FloorToInt((worldPosition.y + ((float)m_data.m_currentHeight * m_data.m_currentCellSize) * .5f) / m_data.m_currentCellSize)
        };
    }

    public Vector3 GetWorldPos(Vector2i gridPos)
    {
        return m_data.m_grid.m_navigationGrid[gridPos.x, gridPos.y].m_worldPos;
    }


    bool IsAccessableForSize(Vector2i gridPos)
    {
        return m_characterSize < (m_data.m_grid.m_navigationGrid[gridPos.x, gridPos.y].m_smallestMaxAccessDistance);
    }

    List<Vector2i> GetNeighbours(Vector2i gridPosition)
    {
        var neighbours = new List<Vector2i>();
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                Vector2i current = gridPosition + new Vector2i { x = x, y = y };
                if (current.x >= 0 && current.y >= 0 && current.x < m_data.m_currentWidth && current.y < m_data.m_currentHeight)
                    neighbours.Add(current);
            }
        }
        return neighbours;
    }
}
