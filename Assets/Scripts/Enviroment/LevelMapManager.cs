using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LevelMapManager : MonoBehaviour
{
	public int m_width;
	public int m_height;
	public LevelMapVisualization m_mapVisualization; 

	private LevelMap m_map = null;

	void Start()
	{
		m_map = GetComponent<LevelMap>();
		m_map.Generate(m_width, m_height);
		m_mapVisualization.GenerateMesh(m_map);
		GeneratePlayerStartPositions();
	}

	public LevelMap GetMap()
	{
		Debug.Assert(m_map, "Trying to use map before it's created");
		return m_map;
	}

	private void GeneratePlayerStartPositions()
	{
		// TODO get actual player count from somewhere
		const int playerPositionCount = 4;

		float distanceToCenter = 0.2f * Mathf.Min(m_map.Width, m_map.Height);
		float angleBetweenPositions = 360.0f / playerPositionCount;
		Vector2 center = new Vector2(m_map.Width, m_map.Height) / 2.0f;

		for (int i = 0; i < playerPositionCount; ++i)
		{
			float angleRadians = i * angleBetweenPositions * Mathf.Deg2Rad;
			Vector2 direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
			Vector2i gridPos = new Vector2i(center + direction * distanceToCenter);

			gridPos = m_map.GetNavGrid().FindClosestAccessiblePosition(gridPos, 0.5f);

			GameObject playerStartPosGo = new GameObject();
			playerStartPosGo.name = "PlayerStartPosition" + i;
			playerStartPosGo.AddComponent<NetworkStartPosition>();
			playerStartPosGo.transform.parent = m_mapVisualization.transform;
			playerStartPosGo.transform.position = MapGrid.GridToWorldPoint(gridPos, -0.5f);
		}
	}

}
