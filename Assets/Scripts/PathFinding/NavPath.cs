using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NavPath : List<Vector2i>
{
}

public struct NavMovementEvalData
{
	public float f;
}

public delegate float MovementEvalFuncDelegate(NavMovementEvalData d);

public class NavPathAgent
{
	private float m_agentRadius; // In grid coordinates
	private NavGrid m_navGrid;
	private MovementEvalFuncDelegate m_del;

	private static Vector2i sm_invalidIndex = new Vector2i(-1, -1);

	public NavPathAgent(float agentRadius, NavGrid navGrid, MovementEvalFuncDelegate del)
	{
		m_agentRadius = agentRadius;
		m_navGrid = navGrid;
		m_del = del;
	}

	public bool CanAccess(Vector2i gridPos)
	{
		return m_navGrid.IsAccessibleForSize(gridPos, m_agentRadius);
	}

	private class Node
	{
		public Node() { g_score = float.MaxValue; }
		public Node(float f, float g) { f_score = f; g_score = g; cameFrom = new Vector2i(); }
		public Node(float f, float g, Vector2i c) { f_score = f; g_score = g; cameFrom = c; }

		public float g_score;
		public float f_score;
		public Vector2i cameFrom;
	}

	public NavPath SeekPath(Vector2i startGridPos, Vector2i endGridPos)
	{
		NavPath path = new NavPath();

		if (startGridPos == endGridPos)
			return path;

		endGridPos = m_navGrid.FindClosestAccessiblePosition(endGridPos, m_agentRadius);

		var closedSet = new Dictionary<Vector2i, Node>();
		var map = new Dictionary<Vector2i, Node>();
		var openSet = new Dictionary<Vector2i, Node>();
		var startNode = new Node(startGridPos.Distance(endGridPos), 0.0f, sm_invalidIndex);

		map[startGridPos] = startNode;
		openSet[startGridPos] = startNode;

		while (openSet.Count > 0)
		{
			var best = openSet.Aggregate((c, n) => c.Value.f_score < n.Value.f_score ? c : n);
			openSet.Remove(best.Key);
			closedSet[best.Key] = best.Value;

			if (best.Key == endGridPos)
			{
				path.Clear();
				Node n = best.Value;
				path.Add(best.Key);
				while (n != null && n.cameFrom != sm_invalidIndex)
				{
					path.Insert(0, n.cameFrom);
					n = map[n.cameFrom];
				}
				return path;
			}

			var neighbours = GetNeighbours(best.Key);
			for (int i = 0; i < neighbours.Count; ++i)
			{
				Vector2i n = neighbours[i];
				if (!CanAccess(n) || closedSet.ContainsKey(n))
					continue;

				NavMovementEvalData d = new NavMovementEvalData();
				d.f = 1.0f;
				float g = best.Value.g_score + m_del(d);
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
					c.f_score = g + n.Distance(endGridPos);
				}

			}
		}

		return path;
	}

	private List<Vector2i> GetNeighbours(Vector2i gridPos)
	{
		var neighbours = new List<Vector2i>();
		for (var x = -1; x <= 1; x++)
		{
			for (var y = -1; y <= 1; y++)
			{
				Vector2i current = gridPos + new Vector2i(x, y);
				if (current.x >= 0 && current.y >= 0 && current.x < m_navGrid.Width && current.y < m_navGrid.Height)
					neighbours.Add(current);
			}
		}
		return neighbours;
	}
}