using UnityEngine;
using System.Collections;

/*
 * World position is origo in case of the cell being unaccessible,
 * always check accessibility first!
*/
public class NavGridCell
{
    public NavGridCell() { _worldPos = Vector3.zero; _accessible = false; _outOfTheScene = true; _movementCost = 1.0f; _smallestMaxAccessDistance = .0f; }

    public float _smallestMaxAccessDistance;
    public Vector3 _worldPos;
    public bool _accessible;
    public bool _outOfTheScene;
    public float _movementCost;
};

public class NavGrid {

    public NavGridCell[,] _navigationGrid; 

    public NavGrid(int w, int h)
    {
        _navigationGrid = new NavGridCell[w, h];
    }
}
