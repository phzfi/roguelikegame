using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelMapVisualization : MonoBehaviour
{
	static public float sm_depth = 0.1f;
	static public float sm_meshHeight = 3.0f;
    public Material gridMaterial;
    public Material wallMaterial;
    public Material topMaterial;
    public Material floorMaterial;
	public Transform gridTransform;

    public GameObject m_torch;
    public float m_torchProbability;

    private GameObject top;
    private GameObject floor;
    private GameObject walls;

    private List<Vector3> vertices;
	private List<int> triangles;
    private List<Vector2> uvs;
    
    private List<Vector3> floorVertices;
    private List<int> floorTriangles;
    private List<Vector2> floorUVs;

    private List<Vector3> wallVertices;
    private List<int> wallTriangles;
    private List<Vector2> wallUVs;

    private System.Random m_rand;

    public void Init(LevelMap map)
    {
        m_rand = new System.Random(map.m_seed.GetHashCode());

        top = new GameObject();
        top.name = "Top";
        top.transform.parent = this.gameObject.transform;
        MeshRenderer topMeshRenderer = top.AddComponent<MeshRenderer>();
        MeshFilter topMeshFilter = top.AddComponent<MeshFilter>();
        topMeshRenderer.material = topMaterial;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        floor = new GameObject();
        floor.name = "Floor";
        floor.transform.parent = this.gameObject.transform;
        MeshRenderer floorMeshRenderer = floor.AddComponent<MeshRenderer>();
        MeshFilter floorMeshFilter = floor.AddComponent<MeshFilter>();
        floorMeshRenderer.material = floorMaterial;
        floorVertices = new List<Vector3>();
        floorTriangles = new List<int>();
        floorUVs = new List<Vector2>();

        walls = new GameObject();
        walls.name = "Walls";
        MeshRenderer meshRenderer = walls.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = walls.AddComponent<MeshFilter>();
        walls.transform.parent = this.gameObject.transform;
        meshRenderer.material = wallMaterial;
        wallVertices = new List<Vector3>();
        wallTriangles = new List<int>();
        wallUVs = new List<Vector2>();
    }

    public void UpdateMesh()
    {
        MeshFilter topMeshFilter = top.GetComponent<MeshFilter>();
        topMeshFilter.mesh.vertices = vertices.ToArray();
        topMeshFilter.mesh.triangles = triangles.ToArray();
        topMeshFilter.mesh.uv = uvs.ToArray();
        topMeshFilter.mesh.RecalculateNormals();

        MeshFilter floorMeshFilter = floor.GetComponent<MeshFilter>();
        floorMeshFilter.mesh.vertices = floorVertices.ToArray();
        floorMeshFilter.mesh.triangles = floorTriangles.ToArray();
        floorMeshFilter.mesh.uv = floorUVs.ToArray();
        floorMeshFilter.mesh.RecalculateNormals();

        MeshFilter wallMeshFilter = walls.GetComponent<MeshFilter>();
        wallMeshFilter.mesh.vertices = wallVertices.ToArray();
        wallMeshFilter.mesh.triangles = wallTriangles.ToArray();
        wallMeshFilter.mesh.uv = wallUVs.ToArray();
        wallMeshFilter.mesh.RecalculateNormals();
    }

    public void MarchingSquaresMesh(LevelMap map)
    {
        Init(map);

        int width = map.Width;
        int height = map.Height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MeshSingle(map, x, y);
            }
        }

        float tileSize = MapGrid.tileSize;
        Vector3 heightVector = new Vector3(0, 0, -sm_meshHeight);

        Vector3 floorTopLeft = MapGrid.GridToWorldPoint(0, height - 1, sm_depth);
        floorTopLeft.x -= tileSize / 2;
        floorTopLeft.y += tileSize / 2;
        Vector3 floorTopRight = MapGrid.GridToWorldPoint(width - 1, height - 1, sm_depth);
        floorTopRight.x += tileSize / 2;
        floorTopRight.y += tileSize / 2;
        Vector3 floorBotLeft = MapGrid.GridToWorldPoint(0, 0, sm_depth);
        floorBotLeft.x -= tileSize / 2;
        floorBotLeft.y -= tileSize / 2;
        Vector3 floorBotRight = MapGrid.GridToWorldPoint(width - 1, 0, sm_depth);
        floorBotRight.x += tileSize / 2;
        floorBotRight.y -= tileSize / 2;

        CreateFloorTriangle(floorBotRight, floorTopLeft, floorTopRight);
        CreateFloorTriangle(floorBotRight, floorBotLeft, floorTopLeft);
        floorUVs.Add(new Vector2(1.0f * width, 0.0f)); floorUVs.Add(new Vector2(0.0f, 1.0f * height));
        floorUVs.Add(new Vector2(1.0f * width, 1.0f * height)); floorUVs.Add(new Vector2(1.0f * width, 0.0f));
        floorUVs.Add(new Vector2(0.0f, 0.0f)); floorUVs.Add(new Vector2(0.0f, 1.0f * height));

        // Create top edge (to fix shadows)
        Vector3 floorTopLeft2 = floorTopLeft + heightVector;
        Vector3 floorTopRight2 = floorTopRight + heightVector;
        CreateFloorTriangle(floorTopLeft2, floorTopLeft, floorTopRight);
        CreateFloorTriangle(floorTopLeft2, floorTopRight, floorTopRight2);
        floorUVs.Add(new Vector2(0.0f, 1.0f * sm_meshHeight)); floorUVs.Add(new Vector2(0.0f, 0.0f));
        floorUVs.Add(new Vector2(1.0f * width, 0.0f)); floorUVs.Add(new Vector2(0.0f, 1.0f * sm_meshHeight));
        floorUVs.Add(new Vector2(1.0f * width, 0.0f)); floorUVs.Add(new Vector2(1.0f * width, 1.0f * sm_meshHeight));

        // Create right edge (to fix shadows)
        Vector3 floorBotRight2 = floorBotRight + heightVector;
        CreateFloorTriangle(floorBotRight, floorBotRight2, floorTopRight);
        CreateFloorTriangle(floorTopRight, floorBotRight2, floorTopRight2);
        floorUVs.Add(new Vector2(0.0f, 0.0f)); floorUVs.Add(new Vector2(0.0f, 1.0f * sm_meshHeight));
        floorUVs.Add(new Vector2(1.0f * width, 0.0f)); floorUVs.Add(new Vector2(1.0f * width, 0.0f));
        floorUVs.Add(new Vector2(0.0f, 1.0f * sm_meshHeight)); floorUVs.Add(new Vector2(1.0f * width, 1.0f * sm_meshHeight));

        UpdateMesh();
        UpdateGrid(map.Size);
    }

	public void MeshCircle(LevelMap map, Vector2i pos, int radius) 
	{
		for (int x = pos.x - radius; x <= pos.x + radius; x++) 
		{
			for (int y = pos.y - radius; y <= pos.y + radius; y++) 
			{
				if (y < map.Height && x < map.Width && x >= 0 && y >= 0) 
				{
					int xdist = x - pos.x;
					int ydist = y - pos.y;
					if (xdist * xdist + ydist * ydist <= radius * radius) 
					{
						MeshSingle (map, x, y);
					}
				}
			}
		}
	}

    public void MeshSingle(LevelMap map, int x, int y)
    {
        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
        {
            Debug.LogError("The requested square in MeshSquare() is out of range: (" + x + ", "  + y + ")");
            return;
        }

        int state = 0;
        if (map.GetTileType(x, y) == MapTileType.Wall) state += 1;
        if (x + 1 == map.Width || map.GetTileType(x + 1, y) == MapTileType.Wall) state += 2;
        if ((y + 1 == map.Height || x + 1 == map.Width) || map.GetTileType(x + 1, y + 1) == MapTileType.Wall) state += 4;
        if (y + 1 == map.Height || map.GetTileType(x, y + 1) == MapTileType.Wall) state += 8;

        TriangulateSingle(x, y, state, map.Width, map.Height);

        if (m_torch != null && (state == 3 || state == 6 || state == 9 || state == 12))
        {
            SpawnTorch(x, y, state);
        }
    }

    private void SpawnTorch(int x, int y, int state)
    {
        float r = (float)m_rand.NextDouble();
        if (r < m_torchProbability)
        {
            Vector3 pos = MapGrid.GridToWorldPoint(x, y, -1.0f);
            Instantiate(m_torch, pos, m_torch.transform.rotation);
        }
    }

    private void TriangulateSingle(int x, int y, int state, int width, int height)
    {
        Vector3 middle = MapGrid.GridToWorldPoint(x, y, sm_depth - sm_meshHeight);
        Vector3 right = new Vector3(MapGrid.tileSize / 2.0f, 0.0f, 0.0f);
        Vector3 up = new Vector3(0.0f, MapGrid.tileSize / 2.0f, 0.0f);
        Vector3 bottomLeft = middle - up - right; Vector3 bottomRight = middle - up + right;
        Vector3 topLeft = middle + up - right; Vector3 topRight = middle + right + up;
        Vector3 centerLeft = middle - right; Vector3 centerRight = middle + right;
        Vector3 centerBottom = middle - up; Vector3 centerTop = middle + up;

        switch (state)
        {
            case 0:
                break;

            // 1 active
            case 1:
                MeshFromPoints(width, height, centerLeft, centerBottom, bottomLeft);
                CreateWall(centerLeft, centerBottom);
                break;
            case 2:
                MeshFromPoints(width, height, bottomRight, centerBottom, centerRight);
                CreateWall(centerBottom, centerRight);
                break;
            case 4:
                MeshFromPoints(width, height, topRight, centerRight, centerTop);
                CreateWall(centerRight, centerTop);
                break;
            case 8:
                MeshFromPoints(width, height, topLeft, centerTop, centerLeft);
                CreateWall(centerTop, centerLeft);
                break;

            // 2 active
            case 3:
                MeshFromPoints(width, height, centerRight, bottomRight, bottomLeft, centerLeft);
                CreateWall(centerLeft, centerRight);
                break;
            case 6:
                MeshFromPoints(width, height, centerTop, topRight, bottomRight, centerBottom);
                CreateWall(centerBottom, centerTop);
                break;
            case 9:
                MeshFromPoints(width, height, topLeft, centerTop, centerBottom, bottomLeft);
                CreateWall(centerTop, centerBottom);
                break;
            case 12:
                MeshFromPoints(width, height, topLeft, topRight, centerRight, centerLeft);
                CreateWall(centerRight, centerLeft);
                break;
            case 5:
                MeshFromPoints(width, height, centerTop, topRight, centerRight, centerBottom, bottomLeft, centerLeft);
                CreateWall(centerLeft, centerTop);
                CreateWall(centerRight, centerBottom);
                break;
            case 10:
                MeshFromPoints(width, height, topLeft, centerTop, centerRight, bottomRight, centerBottom, centerLeft);
                CreateWall(centerTop, centerRight);
                CreateWall(centerBottom, centerLeft);
                break;

            // 3 active
            case 7:
                MeshFromPoints(width, height, centerTop, topRight, bottomRight, bottomLeft, centerLeft);
                CreateWall(centerLeft, centerTop);
                break;
            case 11:
                MeshFromPoints(width, height, topLeft, centerTop, centerRight, bottomRight, bottomLeft);
                CreateWall(centerTop, centerRight);
                break;
            case 13:
                MeshFromPoints(width, height, topLeft, topRight, centerRight, centerBottom, bottomLeft);
                CreateWall(centerRight, centerBottom);
                break;
            case 14:
                MeshFromPoints(width, height, topLeft, topRight, bottomRight, centerBottom, centerLeft);
                CreateWall(centerBottom, centerLeft);
                break;

            // 4 active
            case 15:
                MeshFromPoints(width, height, topLeft, topRight, bottomRight, bottomLeft);
                break;
        }
    }

    private void MeshFromPoints(int width, int height, params Vector3[] points)
    {
        if (points.Length >= 3) CreateTriangle(points[0], points[1], points[2], width, height);
        if (points.Length >= 4) CreateTriangle(points[0], points[2], points[3], width, height);
        if (points.Length >= 5) CreateTriangle(points[0], points[3], points[4], width, height);
        if (points.Length >= 6) CreateTriangle(points[0], points[4], points[5], width, height);
    }

    private void CreateWall(Vector3 a, Vector3 b)
    {
        Vector3 c = a + new Vector3(0.0f, 0.0f, sm_meshHeight);
        Vector3 d = b + new Vector3(0.0f, 0.0f, sm_meshHeight);

        wallVertices.Add(a);
        wallUVs.Add(new Vector2(0.0f, 1.0f * sm_meshHeight));
        wallTriangles.Add(wallVertices.Count - 1);
        wallVertices.Add(c);
        wallUVs.Add(new Vector2(0.0f, 0.0f));
        wallTriangles.Add(wallVertices.Count - 1);
        wallVertices.Add(b);
        wallUVs.Add(new Vector2(1.0f, 1.0f * sm_meshHeight));
        wallTriangles.Add(wallVertices.Count - 1);

        wallVertices.Add(b);
        wallUVs.Add(new Vector2(1.0f, 1.0f * sm_meshHeight));
        wallTriangles.Add(wallVertices.Count - 1);
        wallVertices.Add(c);
        wallUVs.Add(new Vector2(0.0f, 0.0f));
        wallTriangles.Add(wallVertices.Count - 1);
        wallVertices.Add(d);
        wallUVs.Add(new Vector2(1.0f, 0.0f));
        wallTriangles.Add(wallVertices.Count - 1);
    }

    private void CreateTriangle(Vector3 a, Vector3 b, Vector3 c, int width, int height)
    {
        vertices.Add(a);
        AddUV(a, width, height);
        triangles.Add(vertices.Count - 1);
        vertices.Add(b);
        AddUV(b, width, height);
        triangles.Add(vertices.Count - 1);
        vertices.Add(c);
        AddUV(c, width, height);
        triangles.Add(vertices.Count - 1);
    }

    private void AddUV(Vector3 pos, int width, int height)
    {
        Vector3 topRight = MapGrid.GridToWorldPoint(width - 1, height - 1) + new Vector3(MapGrid.tileSize / 2.0f, MapGrid.tileSize / 2.0f, 0.0f);
        Vector3 botLeft = MapGrid.GridToWorldPoint(0, 0) - new Vector3(MapGrid.tileSize / 2.0f, MapGrid.tileSize / 2.0f, 0.0f);
        float u = Mathf.InverseLerp(botLeft.x, topRight.x, pos.x);
        float v = Mathf.InverseLerp(botLeft.y, topRight.y, pos.y);
        uvs.Add(new Vector2(u * width, v * height));
    }

    private void CreateFloorTriangle(Vector3 a, Vector3 b, Vector3 c)
	{
        floorVertices.Add(a);
		floorTriangles.Add(floorVertices.Count - 1);
		floorVertices.Add(b);
		floorTriangles.Add(floorVertices.Count - 1);
		floorVertices.Add(c);
		floorTriangles.Add(floorVertices.Count - 1);
	}

    private void UpdateGrid(Vector2i mapSize)
	{
		if (gridMaterial)
		{
			gridMaterial.SetTextureScale("_MainTex", new Vector2(mapSize.x, mapSize.y));
		}

		if (gridTransform)
		{
			Vector2 size = MapGrid.tileSize * new Vector2(mapSize.x, mapSize.y);
			gridTransform.localScale = new Vector3(size.x, size.y, 1.0f);

			Vector2 offset = 0.5f * size;
			Vector3 position = gridTransform.position;
			position.x = offset.x;
			position.y = offset.y;
            gridTransform.position = position;
        }
	}

}
