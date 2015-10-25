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

    public float _characterSize = .1f;
    public List<Vector3> _path;
    public bool _catmullRom = false;
    public Vector3 _startWorldPos;
    public Vector3 _endWorldPos;

    private NavGridScript _data;
    private bool _init = false;

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
        _data = pathFindingData;
        _init = true;
        _path = new List<Vector3>();
    }

    public void SeekPath(MovementEvalFuncDelegate del, Vector3 startWorldPos, Vector3 endWorldPos)
    {
        if (!_init)
        {
            Debug.LogError("Trying to use NavPath without initialization");
            return;
        }

        _path.Clear();
        _catmullRom = false;
        _startWorldPos = startWorldPos;
        _startWorldPos = startWorldPos;
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
                if (_path == null)
                    _path = new List<Vector3>();
                else
                    _path.Clear();

                Node n = best.Value;
                _path.Add(GetWorldPos(best.Key));
                while (n != null && n.cameFrom != GridPosition.None)
                {
                    _path.Insert(0, GetWorldPos(n.cameFrom));
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
            for (int i = 0; i < _data._spiralSize; i++)
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
            x = Mathf.FloorToInt((worldPosition.x + ((float)_data._currentWidth * _data._currentCellSize) * .5f) / _data._currentCellSize),
            y = Mathf.FloorToInt((worldPosition.y + ((float)_data._currentHeight * _data._currentCellSize) * .5f) / _data._currentCellSize)
        };
    }

    Vector3 GetWorldPos(GridPosition gridPos)
    {
        return _data._grid._navigationGrid[gridPos.x, gridPos.y]._worldPos;
    }


    bool IsAccessableForSize(GridPosition gridPos)
    {
        return _characterSize < (_data._grid._navigationGrid[gridPos.x, gridPos.y]._smallestMaxAccessDistance);
    }

    List<GridPosition> GetNeighbours(GridPosition gridPosition)
    {
        var neighbours = new List<GridPosition>();
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                GridPosition current = gridPosition + new GridPosition { x = x, y = y };
                if (current.x >= 0 && current.y >= 0 && current.x < _data._currentWidth && current.y < _data._currentHeight)
                    neighbours.Add(current);
            }
        }
        return neighbours;
    }

    public bool EditPathToFitCatmullRomSpline()
    {
        if (_path.Count >= 2)
        {
            _catmullRom = true;
            Vector3 lastControlPoint = _path[_path.Count - 1];
            Vector3 secondLastControlPoint = _path[_path.Count - 2];
            Vector3 p = lastControlPoint + (lastControlPoint - secondLastControlPoint);
            _path.Add(p);

            Vector3 firstControlPoint = _path[0];
            Vector3 secondControlPoint = _path[1];
            p = firstControlPoint + (firstControlPoint - secondControlPoint);
            _path.Insert(0,p);
            return true;
        }
        else
            return false;
    }

}
