using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class LevelMapManager : NetworkBehaviour
{
	public int m_width;
	public int m_height;
	public LevelMapVisualization m_mapVisualization;
    public GameObject m_coinsPrefab;
    public int m_coinCount;
    public int m_itemCount;
    public List<GameObject> m_items;
    

	private LevelMap m_map = null;

	void Start()
	{
        
		m_map = GetComponent<LevelMap>();
		m_map.Generate(m_width, m_height);
        //m_mapVisualization.GenerateMesh(m_map);
        m_mapVisualization.MarchingSquaresMesh(m_map);
        GeneratePlayerStartPositions();
        GenerateItems();
        
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

    private void GenerateItems()
    {
        
        List<GameObject> itemsToPlace = new List<GameObject>();

        for (int i = 0; i < m_coinCount; i++)
            itemsToPlace.Add(m_coinsPrefab);

        for (int i = 0; i < m_itemCount; i++)
            itemsToPlace.Add(m_items[i % m_items.Count]);

        // Create dimensions for square grid
        System.Random pseudoRandom = new System.Random(m_map.m_seed.GetHashCode());
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
                index = pseudoRandom.Next(0, dim * dim);
            } while (visited.Contains(index));
            visited.Add(index);

            // Generate a random position within the square
            Vector2i gridPos = new Vector2i(pseudoRandom.Next((index % dim) * widthStep + 1, ((index % dim + 1) * widthStep)),
                                            pseudoRandom.Next((index / dim) * heightStep + 1, (index / dim + 1) * heightStep));
            gridPos = m_map.GetNavGrid().FindClosestAccessiblePosition(gridPos, 0.5f);

            // Skip if inside wall
            if (!m_map.IsAccessible(gridPos.x, gridPos.y))
                continue;
            Vector3 pos = MapGrid.GridToWorldPoint(gridPos, -1.0f);
            
            // Create item and place on map
            GameObject obj = (GameObject)Instantiate(itemsToPlace[i], pos, Quaternion.identity);
            var item = obj.GetComponent<Item>();
            ItemManager.GetID(out item.ID);
            item.m_pos = MapGrid.WorldToGridPoint(pos);
            if (NetworkServer.active)
                NetworkServer.Spawn(obj);
        }
    }

}
