using UnityEngine;
using System.Collections;

// TODO support for grid offset?
public class MapGrid
{
	public const float tileSize = 1.0f;

	public static Vector3 GridToWorldPoint(Vector2i gridPosition, float z = 0.0f)
	{
		return GridToWorldPoint(gridPosition.x, gridPosition.y, z);
	}

	public static Vector3 GridToWorldPoint(int gridX, int gridY, float z = 0.0f)
	{
		float x = ((float)gridX + 0.5f) * tileSize;
		float y = ((float)gridY + 0.5f) * tileSize;
		return new Vector3(x, y, z);
	}

	public static Vector2i WorldToGridPoint(Vector3 worldPosition)
	{
		return WorldToGridPoint(worldPosition.x, worldPosition.y);
	}

	public static Vector2i WorldToGridPoint(float worldX, float worldY)
	{
		int x = Mathf.FloorToInt(worldX / (float)tileSize);
		int y = Mathf.FloorToInt(worldY / (float)tileSize);
		return new Vector2i(x, y);
	}

}
