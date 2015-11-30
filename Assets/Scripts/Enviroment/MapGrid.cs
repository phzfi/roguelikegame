#define SMOOTH_PATH

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO support for grid offset?
public class MapGrid
{
	public const float tileSize = 1.5f;

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
	
	public static List<Vector3> NavPathToWorldSpacePath(NavPath navPath, float worldSpaceZ = 0.0f)
	{
		List<Vector3> worldSpacePath = new List<Vector3>(navPath.Count);

		for (int i = 0; i < navPath.Count; ++i)
		{
			Vector3 worldPos = GridToWorldPoint(navPath[i], worldSpaceZ);

#if SMOOTH_PATH
			if (i > 0)
			{
				Vector3 prevWorldPos = GridToWorldPoint(navPath[i - 1], worldSpaceZ);
				worldSpacePath.Add((worldPos + prevWorldPos) / 2.0f);
			}

			if (i == 0 || i == navPath.Count - 1)
			{
				worldSpacePath.Add(worldPos);
			}
#else
			worldSpacePath.Add(worldPos);
#endif
		}

		return worldSpacePath;
	}

}
