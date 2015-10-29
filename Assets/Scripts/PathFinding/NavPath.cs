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
        public Node(float f, float g) { f_score = f; g_score = g; cameFrom = new GridPosition(); }
        public Node(float f, float g, GridPosition c) { f_score = f; g_score = g; cameFrom = c; }

        public float g_score;
        public float f_score;
        public GridPosition cameFrom;
    }

    public struct GridPosition
	{

		public override string ToString ()
		{
			return string.Format ("[GridPosition {0}, {1}]", x, y);
		}

		public static GridPosition None = new GridPosition {x = -1, y = -1};

		public int x;
		public int y;

		public int Distance(GridPosition other)
		{
			return Mathf.Abs(other.x - x) + Mathf.Abs(other.y - y);
		}

		public static GridPosition operator + (GridPosition p1, GridPosition position)
		{
			return new GridPosition { x = p1.x + position.x, y = p1.y + position.y };
		}

		public override bool Equals (object obj)
		{
			if(!(obj is GridPosition))
				return false;
			var gp = (GridPosition)obj;
			return gp.x == x && gp.y == y;
		}

		public override int GetHashCode ()
		{
			return x.GetHashCode() ^ y.GetHashCode();
		}

		public static bool operator == (GridPosition p1, GridPosition p2)
		{
			return p1.Equals(p2);
		}

		public static bool operator != (GridPosition p1, GridPosition p2)
		{
			return !p1.Equals(p2);
		}
		
	}

    public void Initialize(NavGridScript pathFindingData)
    {
        m_data = pathFindingData;
        m_init = true;
        m_path = new List<Vector3>();
    }

    public void SeekPath(MovementEvalFuncDelegate del, Vector3 startWorldPos, Vector3 endWorldPos)
    {
        if (!m_init)
        {
            Debug.LogError("Trying to use NavPath without initialization");
            return;
        }

        m_path.Clear();
        m_catmullRom = false;
        m_startWorldPos = startWorldPos;
        GridPosition start = GetGridPosition(startWorldPos);
        GridPosition end = GetGridPosition(endWorldPos);
        FindClosetAccessableWithSpiral(ref end);
        
        var closedSet = new Dictionary<GridPosition, Node>(); 
        var map = new Dictionary<GridPosition, Node>();
        var openSet = new Dictionary<GridPosition, Node>();
        var startNode = new Node(start.Distance(end), .0f, GridPosition.None);



        map[start] = startNode;
        openSet[start] = startNode;

        while (openSet.Count > 0)
        {
            var best = openSet.Aggregate((c, n) => c.Value.f_score < n.Value.f_score ? c : n);
            openSet.Remove(best.Key);
            closedSet[best.Key] = best.Value;
            if (best.Key == end)
            {
                if (m_path == null)
                    m_path = new List<Vector3>();
                else
                    m_path.Clear();

                Node n = best.Value;
                m_path.Add(GetWorldPos(best.Key));
                while (n != null && n.cameFrom != GridPosition.None)
                {
                    m_path.Insert(0, GetWorldPos(n.cameFrom));
                    n = map[n.cameFrom];
                }
                return;
            }

            var neighbours = GetNeighbours(best.Key);
            for (int i = 0; i < neighbours.Count; ++i)
            {
                GridPosition n = neighbours[i];
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
    }

    public Vector3 GetClosetAccessibleWorldPositionOnGrid(Vector3 p)
    {
        GridPosition gp = GetGridPosition(p);
        FindClosetAccessableWithSpiral(ref gp);
        return GetWorldPos(gp);
    }

    void FindClosetAccessableWithSpiral(ref GridPosition gp)
    {
        if (!IsAccessableForSize(gp))
        {
            Spiral s = new Spiral();
            for (int i = 0; i < m_data.m_spiralSize; i++)
            {
                int nextX, nextY;
                s.Next(out nextX, out nextY);
                GridPosition n = gp;
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

    GridPosition GetGridPosition(Vector3 worldPosition)
    {
        return new GridPosition
        {
            x = Mathf.FloorToInt((worldPosition.x + ((float)m_data.m_currentWidth * m_data.m_currentCellSize) * .5f) / m_data.m_currentCellSize),
            y = Mathf.FloorToInt((worldPosition.y + ((float)m_data.m_currentHeight * m_data.m_currentCellSize) * .5f) / m_data.m_currentCellSize)
        };
    }

    Vector3 GetWorldPos(GridPosition gridPos)
    {
        return m_data.m_grid.m_navigationGrid[gridPos.x, gridPos.y].m_worldPos;
    }


    bool IsAccessableForSize(GridPosition gridPos)
    {
        return m_characterSize < (m_data.m_grid.m_navigationGrid[gridPos.x, gridPos.y].m_smallestMaxAccessDistance);
    }

    List<GridPosition> GetNeighbours(GridPosition gridPosition)
    {
        var neighbours = new List<GridPosition>();
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                GridPosition current = gridPosition + new GridPosition { x = x, y = y };
                if (current.x >= 0 && current.y >= 0 && current.x < m_data.m_currentWidth && current.y < m_data.m_currentHeight)
                    neighbours.Add(current);
            }
        }
        return neighbours;
    }

    public bool EditPathToFitCatmullRomSpline()
    {
        if (m_path.Count >= 2)
        {
            m_catmullRom = true;
            Vector3 lastControlPoint = m_path[m_path.Count - 1];
            Vector3 secondLastControlPoint = m_path[m_path.Count - 2];
            Vector3 p = lastControlPoint + (lastControlPoint - secondLastControlPoint);
            m_path.Add(p);

            Vector3 firstControlPoint = m_path[0];
            Vector3 secondControlPoint = m_path[1];
            p = firstControlPoint + (firstControlPoint - secondControlPoint);
            m_path.Insert(0,p);
            return true;
        }
        else
            return false;
    }

}
