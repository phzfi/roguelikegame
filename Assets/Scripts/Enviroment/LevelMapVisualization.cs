using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelMapVisualization : MonoBehaviour
{
	static public float sm_depth = 0.1f;
	static public float sm_meshHeight = 3.0f;
	public Material gridMaterial;
	public Transform gridTransform;

	private List<Vector3> vertices;
	private List<int> triangles;
    private List<Vector2> uvs;
    private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    private List<List<int>> edges = new List<List<int>>();
    private HashSet<int> visited = new HashSet<int>();
    private SquareGrid squareGrid;

	public void GenerateMesh(LevelMap map)
	{
		vertices = new List<Vector3>();
		triangles = new List<int>();

		int width = map.Width;
		int height = map.Height;
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

		// Create floor
		CreateTriangle(floorBotRight, floorTopLeft, floorTopRight);
		CreateTriangle(floorBotRight, floorBotLeft, floorTopLeft);

		// Create top edge (to fix shadows)
		Vector3 floorTopLeft2 = floorTopLeft + heightVector;
        Vector3 floorTopRight2 = floorTopRight + heightVector;
		CreateTriangle(floorTopLeft2, floorTopLeft, floorTopRight);
		CreateTriangle(floorTopLeft2, floorTopRight, floorTopRight2);

		// Create right edge (to fix shadows)
		Vector3 floorBotRight2 = floorBotRight + heightVector;
		CreateTriangle(floorBotRight, floorBotRight2, floorTopRight);
		CreateTriangle(floorTopRight, floorBotRight2, floorTopRight2);

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (map.GetTileType(i, j) == MapTileType.Wall)
				{
					Vector3 pos = MapGrid.GridToWorldPoint(i, j, sm_depth);
					Vector3 topLeft = new Vector3(-tileSize / 2, tileSize / 2, 0);
					Vector3 topRight = new Vector3(tileSize / 2, tileSize / 2, 0);
					Vector3 botLeft = new Vector3(-tileSize / 2, -tileSize / 2, 0);
					Vector3 botRight = new Vector3(tileSize / 2, -tileSize / 2, 0);

					// Create walls
					
					if (i + 1 < width && map.GetTileType(i + 1, j) != MapTileType.Wall)
					{
						CreateTriangle(botRight + pos, topRight + pos + heightVector, topRight + pos);
						CreateTriangle(botRight + pos, botRight + pos + heightVector, topRight + pos + heightVector);
					}
					if (j - 1 > 0 && map.GetTileType(i, j - 1) != MapTileType.Wall)
					{
						CreateTriangle(botLeft + pos, botRight + pos + heightVector, botRight + pos);
						CreateTriangle(botLeft + pos, botLeft + pos + heightVector, botRight + pos + heightVector);
					}
					if (i - 1 > 0 && map.GetTileType(i - 1, j) != MapTileType.Wall)
					{
						CreateTriangle(topLeft + pos, botLeft + pos + heightVector, botLeft + pos);
						CreateTriangle(topLeft + pos, topLeft + pos + heightVector, botLeft + pos + heightVector);
					}
					if (j + 1 < height && map.GetTileType(i, j + 1) != MapTileType.Wall)
					{
						CreateTriangle(topRight + pos, topLeft + pos + heightVector, topLeft + pos);
						CreateTriangle(topRight + pos, topRight + pos + heightVector, topLeft + pos + heightVector);
					}

					// Create tops of walls
					CreateTriangle(topLeft + pos + heightVector, topRight + pos + heightVector, botRight + pos + heightVector);
					CreateTriangle(topLeft + pos + heightVector, botRight + pos + heightVector, botLeft + pos + heightVector);

				}
			}
		}

		Mesh mesh = new Mesh();
		mesh.name = "LevelMesh";
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		UpdateGrid(map.Size);
    }

    public void MarchingSquaresMesh(LevelMap map)
    {
        triangleDictionary.Clear();
        edges.Clear();
        visited.Clear();

        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        squareGrid = new SquareGrid(map, MapGrid.tileSize);
        int width = map.Width;
        int height = map.Height;
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

        // Create wall top pieces using marching squares
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        // Create uvs for wall top vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            float u = Mathf.InverseLerp(floorBotLeft.x, floorTopRight.x, vertices[i].x);
            float v = Mathf.InverseLerp(floorBotLeft.y, floorTopRight.y, vertices[i].y);
            uvs.Add(new Vector2(u, v));
        }

        // Create wall sides
        CreateWallMesh();
        
        // Create floor
        CreateTriangle(floorBotRight, floorTopLeft, floorTopRight);
        CreateTriangle(floorBotRight, floorBotLeft, floorTopLeft);
        uvs.Add(new Vector2(1.0f, 0.0f)); uvs.Add(new Vector2(0.0f, 1.0f));
        uvs.Add(new Vector2(1.0f, 1.0f)); uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 0.0f)); uvs.Add(new Vector2(0.0f, 1.0f));

        // Create top edge (to fix shadows)
        Vector3 floorTopLeft2 = floorTopLeft + heightVector;
        Vector3 floorTopRight2 = floorTopRight + heightVector;
        CreateTriangle(floorTopLeft2, floorTopLeft, floorTopRight);
        CreateTriangle(floorTopLeft2, floorTopRight, floorTopRight2);
        uvs.Add(new Vector2(0.0f, 1.0f)); uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 0.0f)); uvs.Add(new Vector2(0.0f, 1.0f));
        uvs.Add(new Vector2(1.0f, 0.0f)); uvs.Add(new Vector2(1.0f, 1.0f));

        // Create right edge (to fix shadows)
        Vector3 floorBotRight2 = floorBotRight + heightVector;
        CreateTriangle(floorBotRight, floorBotRight2, floorTopRight);
        CreateTriangle(floorTopRight, floorBotRight2, floorTopRight2);
        uvs.Add(new Vector2(0.0f, 0.0f)); uvs.Add(new Vector2(0.0f, 1.0f));
        uvs.Add(new Vector2(1.0f, 0.0f)); uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 1.0f)); uvs.Add(new Vector2(1.0f, 1.0f));

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        UpdateGrid(map.Size);
    }

    private void CreateWallMesh()
    {
        CalculateMeshEdges();

        for (int i = 0; i < edges.Count; i++)
        {
            for (int j = 0; j < edges[i].Count - 1; j++)
            {
                int startIndex = vertices.Count;
                vertices.Add(vertices[edges[i][j]]);
                vertices.Add(vertices[edges[i][j + 1]]);
                vertices.Add(vertices[edges[i][j]] + Vector3.forward * sm_meshHeight);
                vertices.Add(vertices[edges[i][j + 1]] + Vector3.forward * sm_meshHeight);

                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(1.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 1.0f));
                uvs.Add(new Vector2(1.0f, 1.0f));

                triangles.Add(startIndex);
                triangles.Add(startIndex + 2);
                triangles.Add(startIndex + 3);

                triangles.Add(startIndex + 3);
                triangles.Add(startIndex + 1);
                triangles.Add(startIndex);
            }
        }
    }

    private void TriangulateSquare(Square square)
    {
        switch (square.state)
        {
            case 0:
                break;

            // 1 node active
            case 1:
                MeshFromNodes(square.centerLeft, square.centerBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromNodes(square.bottomRight, square.centerBottom, square.centerRight);
                break;
            case 4:
                MeshFromNodes(square.topRight, square.centerRight, square.centerTop);
                break;
            case 8:
                MeshFromNodes(square.topLeft, square.centerTop, square.centerLeft);
                break;

            // 2 nodes active
            case 3:
                MeshFromNodes(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 6:
                MeshFromNodes(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
                break;
            case 9:
                MeshFromNodes(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromNodes(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            case 5:
                MeshFromNodes(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
                break;
            case 10:
                MeshFromNodes(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            // 3 nodes active
            case 7:
                MeshFromNodes(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 11:
                MeshFromNodes(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromNodes(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromNodes(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            // 4 nodes active
            case 15:
                MeshFromNodes(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                visited.Add(square.topLeft.vertex);
                visited.Add(square.topRight.vertex);
                visited.Add(square.bottomRight.vertex);
                visited.Add(square.bottomLeft.vertex);
                break;
        }
    }

    private void MeshFromNodes(params Node[] nodes)
    {
        AssignVertices(nodes);
        if (nodes.Length >= 3) CreateTriangle(nodes[0], nodes[1], nodes[2]);
        if (nodes.Length >= 4) CreateTriangle(nodes[0], nodes[2], nodes[3]);
        if (nodes.Length >= 5) CreateTriangle(nodes[0], nodes[3], nodes[4]);
        if (nodes.Length >= 6) CreateTriangle(nodes[0], nodes[4], nodes[5]);
    }

    private void AssignVertices(Node[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].vertex == -1)
            {
                nodes[i].vertex = vertices.Count;
                vertices.Add(nodes[i].pos);
            }
        }
    }

    private void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertex);
        triangles.Add(b.vertex);
        triangles.Add(c.vertex);

        Triangle triangle = new Triangle(a.vertex, b.vertex, c.vertex);
        AddTriangleToDictionary(triangle.vertexA, triangle);
        AddTriangleToDictionary(triangle.vertexB, triangle);
        AddTriangleToDictionary(triangle.vertexC, triangle);
    }

    private void CreateTriangle(Vector3 a, Vector3 b, Vector3 c)
	{
		vertices.Add(a);
		triangles.Add(vertices.Count - 1);
		vertices.Add(b);
		triangles.Add(vertices.Count - 1);
		vertices.Add(c);
		triangles.Add(vertices.Count - 1);
	}

    private void AddTriangleToDictionary(int vertex, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertex))
            triangleDictionary[vertex].Add(triangle);
        else
        {
            List<Triangle> triList = new List<Triangle>();
            triList.Add(triangle);
            triangleDictionary.Add(vertex, triList);
        }
    }

    private void CalculateMeshEdges()
    {
        for (int vertex = 0; vertex < vertices.Count; vertex++)
        {
            if (!visited.Contains(vertex))
            {
                int newEdgeVertex = GetConnectedEdgeVertex(vertex);
                if (newEdgeVertex != -1)
                {
                    visited.Add(vertex);
                    List<int> edge = new List<int>();
                    edge.Add(vertex);
                    edges.Add(edge);
                    FollowEdge(newEdgeVertex, edges.Count - 1);
                    edges[edges.Count - 1].Add(vertex);
                }
            }
        }
    }

    private void FollowEdge(int vertex, int edge)
    {
        int vtx = vertex;
        while (vtx != -1)
        {
            edges[edge].Add(vtx);
            visited.Add(vtx);
            vtx = GetConnectedEdgeVertex(vtx);
        }
    }

    private int GetConnectedEdgeVertex(int vertex)
    {
        List<Triangle> tris = triangleDictionary[vertex];
        for (int i = 0; i < tris.Count; i++)
        {
            Triangle tri = tris[i];
            for (int j = 0; j < 3; j++)
            {
                int vtx = tri[j];
                if (vtx != vertex && !visited.Contains(vtx) && IsWallEdge(vertex, vtx))
                    return vtx;
            }
        }
        return -1;
    }

    private bool IsWallEdge(int vertexA, int vertexB)
    {
        List<Triangle> triangles = triangleDictionary[vertexA];
        int sharedTris = 0;

        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i].Contains(vertexB))
            {
                sharedTris++;
                if (sharedTris > 1) break;
            }
        }

        return sharedTris == 1;
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

    struct Triangle
    {
        public int vertexA;
        public int vertexB;
        public int vertexC;

        public Triangle(int a, int b, int c)
        {
            vertexA = a;
            vertexB = b;
            vertexC = c;
        }

        public int this[int idx]
        {
            get
            {
                if (idx == 0) return vertexA;
                else if (idx == 1) return vertexB;
                else if (idx == 2) return vertexC;
                else return -1;
            }
        }

        public bool Contains(int vertex)
        {
            return vertex == vertexA || vertex == vertexB || vertex == vertexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(LevelMap map, float squareSize)
        {
            int w = map.Width;
            int h = map.Height;

            CornerNode[,] cornerNodes = new CornerNode[w + 1, h + 1];

            Vector3 offset = new Vector3(-MapGrid.tileSize / 2.0f, -MapGrid.tileSize / 2.0f, 0);

            for (int x = 0; x < w + 1; x++)
            {
                for (int y = 0; y < h + 1; y++)
                {
                    Vector3 pos = MapGrid.GridToWorldPoint(x, y, sm_depth - sm_meshHeight) + offset;
                    cornerNodes[x, y] = new CornerNode(pos, x == w || y == h || map.GetTileType(x, y) == MapTileType.Wall, squareSize);
                }
            }

            squares = new Square[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    squares[x, y] = new Square(cornerNodes[x, y + 1], cornerNodes[x + 1, y + 1], cornerNodes[x + 1, y], cornerNodes[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public CornerNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int state;

        public Square(CornerNode topLeft_, CornerNode topRight_, CornerNode bottomRight_, CornerNode bottomLeft_)
        {
            topLeft = topLeft_;
            topRight = topRight_;
            bottomRight = bottomRight_;
            bottomLeft = bottomLeft_;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;

            if (topLeft.active) state += 8;
            if (topRight.active) state += 4;
            if (bottomRight.active) state += 2;
            if (bottomLeft.active) state += 1;
        }

    }

    public class Node
    {
        public Vector3 pos;
        public int vertex = -1;

        public Node(Vector3 pos_)
        {
            pos = pos_;
        }
    }

    public class CornerNode : Node
    {
        public bool active;
        public Node above, right;

        public CornerNode(Vector3 pos_, bool active_, float squareSize) : base(pos_)
        {
            active = active_;
            above = new Node(pos + Vector3.up * squareSize / 2.0f);
            right = new Node(pos + Vector3.right * squareSize / 2.0f);
        }

    }


}
