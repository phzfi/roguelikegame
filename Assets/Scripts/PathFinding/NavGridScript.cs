using UnityEngine;
using System.Collections;
using System;

public class NavGridScript : Singleton<NavGridScript> {

    protected NavGridScript() { }

    public float _width;
    public float _height;
    public float _cellSize = 1.0f;
    public float _spiralSize = 20;
    public NavGrid _grid;
    public int _currentWidth;
    public int _currentHeight;
    public float _currentCellSize;
    private Vector2 _BottomCornerWorldPosition;
    private LayerMask _AccessibleLayer;
    private LayerMask _invertAccessibleLayer;
    private LayerMask _invertEnviromentMask;

    public LayerMask AccessibleMask
    {
        get { return _AccessibleLayer;  }
    }

    public LayerMask invertEnviromentMask
    {
        get { return _invertEnviromentMask; }
    }

    public LayerMask invertAccesbileEnviromentMask
    {
        get { return _invertAccessibleLayer; }
    }

    void Awake()
    {
        GenerateNavGrid();
    }

    void OnDisable()
    {
    }

    public void GenerateNavGrid()
    {
        UpdateEnviromentMasks();
        _currentWidth = Mathf.CeilToInt(_width / _cellSize) * 2 + 1;
        _currentHeight = Mathf.CeilToInt(_height / _cellSize) * 2 + 1;
        var to_left_bottom_x = _width + .5f * _cellSize;
        var to_left_bottom_y = _height + .5f * _cellSize;
        _BottomCornerWorldPosition = new Vector2(to_left_bottom_x, to_left_bottom_y) * -1.0f;
        _grid = new NavGrid(_currentWidth, _currentHeight);
        RunRayCastingLoop(ref _grid, Camera.main.transform.position.z);
        RunPostProcessSpiral(ref _grid);
    }

    public void RunRayCastingLoop(ref NavGrid g, float d)
    {
        for (int x = 0; x < _currentWidth; x++)
        {
            for (int y = 0; y < _currentHeight; y++)
            {
                Vector3 currentPosition = new Vector3(-1.0f * _width + x * _currentCellSize, -1.0f * _height + y * _currentCellSize, d);
                RaycastHit hit;
                NavGridCell cell = new NavGridCell();
                if (Physics.Raycast(currentPosition, Vector3.forward, out hit, 100.0f, _invertEnviromentMask))
                {
                    cell._worldPos = hit.point;
                    cell._outOfTheScene = false;
                    if (hit.collider.gameObject.layer == _AccessibleLayer)
                        cell._accessible = true;
                }
                g._navigationGrid[x, y] = cell;
            }
        } 
    }

    public void RunPostProcessSpiral(ref NavGrid g)
    {
        for (int x = 0; x < _currentWidth; ++x)
        {
            for (int y = 0; y < _currentHeight; ++y)
            {
                NavGridCell cell = g._navigationGrid[x, y];
                if (!cell._accessible)
                    continue;
                Spiral s = new Spiral();
                bool hit = false;
                for (int i = 0; i < _spiralSize; i++)
                {
                    int nextX, nextY;
                    s.Next(out nextX, out nextY);
                    NavGridCell n = g._navigationGrid[(x + nextX), (y + nextY)];
                    if (!n._accessible)
                    {
                        Vector3 d = n._worldPos - cell._worldPos;
                        d.z = 0;
                        cell._smallestMaxAccessDistance = d.magnitude;
                        hit = true;
                        break;
                    }
                }
                if (!hit)
                {
                    int nextX, nextY;
                    s.Next(out nextX, out nextY);
                    cell._smallestMaxAccessDistance = (new Vector2(nextX, nextY)).magnitude;
                }
            }
        }
    }

    public bool IsGridGenerated(out int w, out int h)
    {
        w = _currentWidth/2;
        h = _currentHeight/2;
        return _grid != null;
    }

    public void DeleteNavMesh() { _grid = null; GC.Collect(); }

    void OnDrawGizmosSelected()
    {
        if (_grid == null)
            return;

        for (var x = 0; x < _currentWidth; x++)
        {
            for (var y = 0; y < _currentHeight; y++)
            {
                NavGridCell cell = _grid._navigationGrid[x, y];
                if (cell._outOfTheScene)
                    continue;
                Gizmos.color = cell._accessible ? Color.green : Color.red;
                Vector3 drawPosition = cell._worldPos + Vector3.back * .25f;
                Vector3 s = Vector3.one * _currentCellSize * 0.7f;
                if(cell._accessible)
                    s.z = cell._smallestMaxAccessDistance;
                Gizmos.DrawCube(drawPosition, s);
            }
        }
    }

    public void ClearPatrolRoutes()
    {
        int c = transform.childCount;
        for (int i = c; i > 0; i--)
        {
            GameObject.DestroyImmediate(transform.GetChild(i-1).gameObject);
        }
    }

    public void AccessableCheckUsingWorldPosition(ref float x, ref float y)
    {
        var x_from_bottom_of_grid = (x + _width ) / _cellSize + .5f;
        var y_from_bottom_of_grid = (y + _height ) / _cellSize + .5f;
        var index_x = Mathf.Floor(x_from_bottom_of_grid);
        var index_y = Mathf.Floor(y_from_bottom_of_grid);
        x = (index_x + .5f) * _cellSize;
        y = (index_y + .5f) * _cellSize;
    }

    public bool IsWorldPositionAccessable(ref float x, ref float y)
    {
        var x_from_bottom_of_grid = (x + _width) / _cellSize + .5f;
        var y_from_bottom_of_grid = (y + _height) / _cellSize + .5f;
        var index_x = Mathf.FloorToInt(x_from_bottom_of_grid);
        var index_y = Mathf.FloorToInt(y_from_bottom_of_grid);
        x = _BottomCornerWorldPosition.x + ((float)index_x + .5f) * _cellSize;
        y = _BottomCornerWorldPosition.y + ((float)index_y + .5f) * _cellSize;

        if (index_x < 0 || index_x >= _grid._navigationGrid.GetLength(0))
            return false;
        else if (index_y < 0 || index_y >= _grid._navigationGrid.GetLength(1))
            return false;
        else
            return _grid._navigationGrid[index_x, index_y]._accessible; 
    }

    private void UpdateEnviromentMasks()
    {
        _AccessibleLayer = LayerMask.NameToLayer("EnviromentAccessible");
        _invertAccessibleLayer = 1 << _AccessibleLayer;
        _invertEnviromentMask = _invertAccessibleLayer | 1 << LayerMask.NameToLayer("EnviromentUnaccessible");
    }
}
