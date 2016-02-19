using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : Singleton<GameLogic>
{
    [Header("Map parameters")]
    public int m_width;
    public int m_height;
    public GameObject m_coinsPrefab;
    public int m_coinCount;
    public int m_itemCount;
    public List<GameObject> m_items;
    [Header("Draw debug gizmos for the map")]
    public bool m_drawDebug = false;

    [Header("Map randomization parameters")]
    public bool m_useRandomSeed;
    public string m_seed = System.DateTime.Now.ToString();

    [Range(0, 100)]
    public int m_randomFillPercent;
    public int m_smoothingIterations;

    public float m_frequency;
    public int m_roomThresholdSize;
    public int m_passageWidth;

    [Range(0, 25)]
    public int m_spaceCount;
    public int m_maxSpaceSize;
    public int m_minSpaceSize;
    public int m_meanSpaceSize;
    public float m_standardDeviation;

    protected GameLogic() { }

    private LevelMap m_map = null;
    private List<Vector2i> m_playerStartingPositions = new List<Vector2i>();

    public void Initialize(int numberOfPlayers)
    {
        LevelMapManager mapManager = FindObjectOfType<LevelMapManager>();
        m_map = new LevelMap(true);
        m_map.Generate(this);
        GeneratePlayerStartPositions(numberOfPlayers);
    }

    public OutputConnectMessage PlayerAdd(int playerIndex, ref VisibilityOutputOrder order)
    {
        OutputConnectMessage msg = new OutputConnectMessage();
        msg.m_mapSize.x = m_width;
        msg.m_mapSize.y = m_height;
        List<Vector2i> positions = new List<Vector2i>();
        List<int> tiles = new List<int>();
        List<int> indices = new List<int>();

        m_map.GetAllVisibleTiles(m_playerStartingPositions[playerIndex],
            ref positions,
            ref tiles,
            ref indices);
        order.SetData(positions.Count, positions, tiles, indices);
        return msg;
    }

    public void PlayerMovement(MovementInputOrder order, int playerIndex)
    {

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
            /*GameObject obj = (GameObject)Instantiate(itemsToPlace[i], pos, Quaternion.identity);
            var item = obj.GetComponent<Item>();
            ItemManager.GetID(out item.ID);
            item.m_pos = MapGrid.WorldToGridPoint(pos);
            NetworkServer.Spawn(obj);*/
        }
    }

    private void GeneratePlayerStartPositions(int numberOfPlayers)
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
            m_playerStartingPositions.Add(gridPos);

            /*GameObject playerStartPosGo = new GameObject();
            playerStartPosGo.name = "PlayerStartPosition" + i;
            playerStartPosGo.AddComponent<NetworkStartPosition>();
            playerStartPosGo.transform.parent = m_mapVisualization.transform;
            playerStartPosGo.transform.position = MapGrid.GridToWorldPoint(gridPos, -0.5f);*/
        }
    }

    void OnDrawGizmos()
    {
        if (!m_drawDebug)
            return;

        for(int y = 0; y < m_map.Height; ++y)
        {
            for (int x = 0; x <m_map.Width; ++x)
            {
                MapTileType tile = m_map.GetTileType(x, y);
                switch (tile)
                {
                    case MapTileType.Wall:
                        Gizmos.color = Color.blue;
                        break;
                    case MapTileType.Floor:
                        Gizmos.color = Color.green;
                        break;
                    default:
                        Gizmos.color = Color.red;
                        break;
                }
                Gizmos.DrawSphere(new Vector3(x*1.5f + .75f, y * 1.5f + .75f, .0f), .35f);
            }
        }
    }
}
