using UnityEngine;
using System.Collections;

public class LevelMapUtilities
{
    public static int GetMeshVisualizationIndex(LevelMap map, int x, int y)
    {
        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
        {
            Debug.LogError("The requested square in MeshSquare() is out of range: (" + x + ", " + y + ")");
            return 0;
        }

        int state = 0;
        if (map.GetTileType(x, y) == MapTileType.Wall) state += 1;
        if (x + 1 == map.Width || map.GetTileType(x + 1, y) == MapTileType.Wall) state += 2;
        if ((y + 1 == map.Height || x + 1 == map.Width) || map.GetTileType(x + 1, y + 1) == MapTileType.Wall) state += 4;
        if (y + 1 == map.Height || map.GetTileType(x, y + 1) == MapTileType.Wall) state += 8;

        return state;
    }
}


