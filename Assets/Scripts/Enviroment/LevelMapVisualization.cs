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

    private GameObject top;
    private GameObject floor;
    private GameObject walls;

    private List<Vector3> vertices;
	private List<int> triangles;
    private List<Vector2> uvs;
    private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    private List<List<int>> edges = new List<List<int>>();
    private HashSet<int> visited = new HashSet<int>();
    private SquareGrid squareGrid;

    private List<Vector3> floorVertices;
    private List<int> floorTriangles;
    private List<Vector2> floorUVs;

    private List<Vector3> wallVertices;
    private List<int> wallTriangles;
    private List<Vector2> wallUVs;

    public void Init(LevelMap map)
    {
        triangleDictionary.Clear();
        edges.Clear();
        visited.Clear();
        
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
            uvs.Add(new Vector2(u * map.Width, v * map.Height));
        }

        // Create walls
        CreateWalls();

        // Create floor
        
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

    public void NewMarcher(LevelMap map)
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

        UpdateMesh();
        UpdateGrid(map.Size);
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
                break;
            case 2:
                MeshFromPoints(width, height, bottomRight, centerBottom, centerRight);
                break;
            case 4:
                MeshFromPoints(width, height, topRight, centerRight, centerTop);
                break;
            case 8:
                MeshFromPoints(width, height, topLeft, centerTop, centerLeft);
                break;

            // 2 active
            case 3:
                MeshFromPoints(width, height, centerRight, bottomRight, bottomLeft, centerLeft);
                break;
            case 6:
                MeshFromPoints(width, height, centerTop, topRight, bottomRight, centerBottom);
                break;
            case 9:
                MeshFromPoints(width, height, topLeft, centerTop, centerBottom, bottomLeft);
                break;
            case 12:
                MeshFromPoints(width, height, topLeft, topRight, centerRight, centerLeft);
                break;
            case 5:
                MeshFromPoints(width, height, centerTop, topRight, centerRight, centerBottom, bottomLeft, centerLeft);
                break;
            case 10:
                MeshFromPoints(width, height, topLeft, centerTop, centerRight, bottomRight, centerBottom, centerLeft);
                break;

            // 3 active
            case 7:
                MeshFromPoints(width, height, centerTop, topRight, bottomRight, bottomLeft, centerLeft);
                break;
            case 11:
                MeshFromPoints(width, height, topLeft, centerTop, centerRight, bottomRight, bottomLeft);
                break;
            case 13:
                MeshFromPoints(width, height, topLeft, topRight, centerRight, centerBottom, bottomLeft);
                break;
            case 14:
                MeshFromPoints(width, height, topLeft, topRight, bottomRight, centerBottom, centerLeft);
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

    private void CreateWalls()
    { 
        CalculateMeshEdges();

        for (int i = 0; i < edges.Count; i++)
        {
            for (int j = 0; j < edges[i].Count - 1; j++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[edges[i][j]]);
                wallVertices.Add(vertices[edges[i][j + 1]]);
                wallVertices.Add(vertices[edges[i][j]] + Vector3.forward * sm_meshHeight);
                wallVertices.Add(vertices[edges[i][j + 1]] + Vector3.forward * sm_meshHeight);

                wallUVs.Add(new Vector2(0.0f, 0.0f));
                wallUVs.Add(new Vector2(1.0f, 0.0f));
                wallUVs.Add(new Vector2(0.0f, 1.0f * sm_meshHeight));
                wallUVs.Add(new Vector2(1.0f, 1.0f * sm_meshHeight));

                wallTriangles.Add(startIndex);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex);
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
