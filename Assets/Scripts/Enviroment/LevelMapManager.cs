using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelMapManager : MonoBehaviour {

    private LevelMap m_map;
    public bool m_debug;
    public int m_width;
    public int m_height;
    public float m_squareSize = 1.0f;
    public float m_meshHeight = 3.0f;
    public float m_depth = 1.0f;

    List<Vector3> vertices;
    List<int> triangles;
    

	void Start ()
    {
        m_map = GetComponent<LevelMap>();
        m_map.Generate();
        NavGridScript nav = GameObject.FindObjectOfType<NavGridScript>();
        // Generate navGrid from map -- broken
        nav.GenerateFromMap(m_map);
        GenerateMesh();
    }
	
	// Update is called once per frame
	void Update ()
    {
        
	}
    
    void GenerateMesh()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        Vector3 floorTopLeft = new Vector3(-m_width / 2, -m_height / 2, m_depth);
        Vector3 floorTopRight = new Vector3(m_width / 2, -m_height / 2, m_depth);
        Vector3 floorBotLeft = new Vector3(-m_width / 2, m_height / 2, m_depth);
        Vector3 floorBotRight = new Vector3(m_width / 2, m_height / 2, m_depth);
        // Create floor
        CreateTriangle(floorBotRight, floorTopRight, floorTopLeft);
        CreateTriangle(floorBotRight, floorTopLeft, floorBotLeft);


        for (int i = 0; i < m_width; i++)
        {
            for (int j = 0; j < m_height; j++)
            {
                if (m_map.GetTileType(i, j) == MapTile.Wall)
                {
                    Vector3 pos = new Vector3(-m_width / 2 + i * m_squareSize, -m_height / 2 + j * m_squareSize, m_depth);
                    Vector3 topLeft = new Vector3(-m_squareSize / 2, m_squareSize / 2, 0);
                    Vector3 topRight = new Vector3(m_squareSize / 2, m_squareSize / 2, 0);
                    Vector3 botLeft = new Vector3(-m_squareSize / 2, -m_squareSize / 2, 0);
                    Vector3 botRight = new Vector3(m_squareSize / 2, -m_squareSize / 2, 0);

                    // Create walls
                    Vector3 heightVector = new Vector3(0, 0, -m_meshHeight);
					if (i + 1 < m_width && m_map.GetTileType(i + 1, j) != MapTile.Wall)
                    {
                        CreateTriangle(botRight + pos, topRight + pos + heightVector, topRight + pos);
                        CreateTriangle(botRight + pos, botRight + pos + heightVector, topRight + pos + heightVector);
                    }
                    if (j - 1 > 0 && m_map.GetTileType(i, j - 1) != MapTile.Wall)
                    {
                        CreateTriangle(botLeft + pos, botRight + pos + heightVector, botRight + pos);
                        CreateTriangle(botLeft + pos, botLeft + pos + heightVector, botRight + pos + heightVector);
                    }
                    if (i - 1 > 0 && m_map.GetTileType(i - 1, j) != MapTile.Wall)
                    {
                        CreateTriangle(topLeft + pos, botLeft + pos + heightVector, botLeft + pos);
                        CreateTriangle(topLeft + pos, topLeft + pos + heightVector, botLeft + pos + heightVector);
                    }
                    if (j + 1 < m_height && m_map.GetTileType(i, j + 1) != MapTile.Wall)
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
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void CreateTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        vertices.Add(a);
        triangles.Add(vertices.Count - 1);
        vertices.Add(b);
        triangles.Add(vertices.Count - 1);
        vertices.Add(c);
        triangles.Add(vertices.Count - 1);
    }
}
