using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelMapVisualization : MonoBehaviour
{
	public float m_depth = 1.0f;
	public float m_meshHeight = 3.0f;

	private List<Vector3> vertices;
	private List<int> triangles;

	public void GenerateMesh(LevelMap map)
	{
		vertices = new List<Vector3>();
		triangles = new List<int>();

		int width = map.Width;
		int height = map.Height;

		Vector3 floorTopLeft = MapGrid.GridToWorldPoint(0, height, m_depth);
		Vector3 floorTopRight = MapGrid.GridToWorldPoint(width, height, m_depth);
		Vector3 floorBotLeft = MapGrid.GridToWorldPoint(0, 0, m_depth);
		Vector3 floorBotRight = MapGrid.GridToWorldPoint(width, 0, m_depth);

		// Create floor
		CreateTriangle(floorBotRight, floorTopLeft, floorTopRight);
		CreateTriangle(floorBotRight, floorBotLeft, floorTopLeft);

		float tileSize = MapGrid.tileSize;

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (map.GetTileType(i, j) == MapTileType.Wall)
				{
					Vector3 pos = MapGrid.GridToWorldPoint(i, j, m_depth);
					Vector3 topLeft = new Vector3(-tileSize / 2, tileSize / 2, 0);
					Vector3 topRight = new Vector3(tileSize / 2, tileSize / 2, 0);
					Vector3 botLeft = new Vector3(-tileSize / 2, -tileSize / 2, 0);
					Vector3 botRight = new Vector3(tileSize / 2, -tileSize / 2, 0);

					// Create walls
					Vector3 heightVector = new Vector3(0, 0, -m_meshHeight);
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
