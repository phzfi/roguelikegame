using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelMapVisualization : MonoBehaviour
{
	public float m_depth = 1.0f;
	public float m_meshHeight = 3.0f;
	public Material gridMaterial;
	public Transform gridTransform;

	private List<Vector3> vertices;
	private List<int> triangles;

	public void GenerateMesh(LevelMap map)
	{
		vertices = new List<Vector3>();
		triangles = new List<int>();

		int width = map.Width;
		int height = map.Height;
		float tileSize = MapGrid.tileSize;
		Vector3 heightVector = new Vector3(0, 0, -m_meshHeight);

		Vector3 floorTopLeft = MapGrid.GridToWorldPoint(0, height - 1, m_depth);
		floorTopLeft.x -= tileSize / 2;
		floorTopLeft.y += tileSize / 2;
		Vector3 floorTopRight = MapGrid.GridToWorldPoint(width - 1, height - 1, m_depth);
		floorTopRight.x += tileSize / 2;
		floorTopRight.y += tileSize / 2;
		Vector3 floorBotLeft = MapGrid.GridToWorldPoint(0, 0, m_depth);
		floorBotLeft.x -= tileSize / 2;
		floorBotLeft.y -= tileSize / 2;
		Vector3 floorBotRight = MapGrid.GridToWorldPoint(width - 1, 0, m_depth);
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
					Vector3 pos = MapGrid.GridToWorldPoint(i, j, m_depth);
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

	private void CreateTriangle(Vector3 a, Vector3 b, Vector3 c)
	{
		vertices.Add(a);
		triangles.Add(vertices.Count - 1);
		vertices.Add(b);
		triangles.Add(vertices.Count - 1);
		vertices.Add(c);
		triangles.Add(vertices.Count - 1);
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
