using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

public class LevelMapManager : NetworkBehaviour
{
	public int m_width;
	public int m_height;
	public LevelMapVisualization m_mapVisualization;
    public GameObject m_coinsPrefab;
    public int m_coinCount;
    public int m_itemCount;
    public List<GameObject> m_items;
    public int m_enemyCount;
    public List<GameObject> m_enemies;
    public GameObject m_boss;
    

	private LevelMap m_map = null;
    private List<Vector2i> m_occupiedPositions;
    private System.Random m_rand;

	void Awake()
	{
		m_map = GetComponent<LevelMap>();
		m_map.Generate(m_width, m_height);
        m_mapVisualization.MarchingSquaresMesh(m_map);
        m_rand = new System.Random(m_map.m_seed.GetHashCode());
        m_occupiedPositions = new List<Vector2i>();
        GeneratePlayerStartPositions();
        //GenerateItems();
        StartCoroutine(DelayEnemySpawn());
        StartCoroutine(DelayItemGenerate());
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

            m_occupiedPositions.Add(gridPos);

			GameObject playerStartPosGo = new GameObject();
			playerStartPosGo.name = "PlayerStartPosition" + i;
			playerStartPosGo.AddComponent<NetworkStartPosition>();
			playerStartPosGo.transform.parent = m_mapVisualization.transform;
			playerStartPosGo.transform.position = MapGrid.GridToWorldPoint(gridPos, -0.5f);
		}
	}

	IEnumerator DelayItemGenerate() // coroutine that waits until all clients have sent their input for this turn, then finishes the server side turn
	{
		while (true)
		{
			if (SyncManager.IsServer)
			{
				GenerateItems();
                yield break;
			}
			else if (SyncManager.sm_running)
			{
				yield break;
			}

			yield return null;
		}
	}

    IEnumerator DelayEnemySpawn() // coroutine that waits until all clients have sent their input for this turn, then finishes the server side turn
    {
        while (true)
        {
            if (SyncManager.IsServer)
            {
                SpawnEnemies();
                yield break;
            }
            else if (SyncManager.sm_running)
            {
                yield break;
            }

            yield return null;
        }
    }

    private void GenerateItems()
    {
        
        List<GameObject> itemsToPlace = new List<GameObject>();

        for (int i = 0; i < m_coinCount; i++)
            itemsToPlace.Add(m_coinsPrefab);

        for (int i = 0; i < m_itemCount; i++)
            itemsToPlace.Add(m_items[i % m_items.Count]);

        // Create dimensions for square grid
        int dim = Mathf.CeilToInt(Mathf.Sqrt(itemsToPlace.Count));
        int widthStep = (m_width - 1) / dim;
        int heightStep = (m_height - 1) / dim;
        List<int> visited = new List<int>();
        int index;

        for (int i = 0; i < itemsToPlace.Count; i++)
        {
            // Find an unvisited square
            do
            {
                index = m_rand.Next(0, dim * dim);
            } while (visited.Contains(index));
            visited.Add(index);

            // Generate a random position within the square
            Vector2i gridPos = new Vector2i(m_rand.Next((index % dim) * widthStep + 1, ((index % dim + 1) * widthStep)),
                                            m_rand.Next((index / dim) * heightStep + 1, (index / dim + 1) * heightStep));
            gridPos = m_map.GetNavGrid().FindClosestAccessiblePosition(gridPos, 0.5f);

            // Skip if inside wall
            if (!m_map.IsAccessible(gridPos.x, gridPos.y) && !m_occupiedPositions.Contains(gridPos))
                continue;

            m_occupiedPositions.Add(gridPos);
            Vector3 pos = MapGrid.GridToWorldPoint(gridPos, -1.0f);
            
            // Create item and place on map
            GameObject obj = (GameObject)Instantiate(itemsToPlace[i], pos, Quaternion.identity);
            var item = obj.GetComponent<Item>();
            ItemManager.GetID(out item.ID);
            item.m_pos = MapGrid.WorldToGridPoint(pos);
            NetworkServer.Spawn(obj);
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i <= m_enemyCount; ++i)
        {
            Vector2i gridPos = new Vector2i(m_rand.Next(1, m_map.Width - 2), m_rand.Next(1, m_map.Height - 2));
            gridPos = m_map.GetNavGrid().FindClosestAccessiblePosition(gridPos, 0.5f);

            // Skip if inside wall
            if (!m_map.IsAccessible(gridPos.x, gridPos.y) && !m_occupiedPositions.Contains(gridPos))
                continue;

            m_occupiedPositions.Add(gridPos);
            Vector3 pos = MapGrid.GridToWorldPoint(gridPos, 0.0f);

            int idx = m_rand.Next(0, m_enemies.Count - 1);

            // Create item and place on map
            GameObject obj;
            if (i != m_enemyCount)
            {
                obj = (GameObject)Instantiate(m_enemies[idx], pos, Quaternion.identity);
            } else
            {
                obj = (GameObject)Instantiate(m_boss, pos, Quaternion.identity);
            }
            NetworkServer.Spawn(obj);
        }
    }
}
