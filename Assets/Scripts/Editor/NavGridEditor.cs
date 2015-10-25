using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(NavGridScript))]
public class NavGridEditor : Editor {

    private NavGridScript _obj;
    private bool existFlag = false;

    public void OnEnable()
    {
        _obj = (NavGridScript)target;
    }

    public override void OnInspectorGUI()
    {
        int w; int h;
        existFlag = _obj.IsGridGenerated(out w, out h);

        GUILayout.BeginHorizontal();
        GUILayout.Label("NavGrid width");
        _obj._width = EditorGUILayout.FloatField(_obj._width, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("NavGrid height");
        _obj._height = EditorGUILayout.FloatField(_obj._height, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("NavGrid Cell size");
        _obj._cellSize = EditorGUILayout.FloatField(_obj._cellSize, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("NavGrid Spiral size");
        _obj._spiralSize = EditorGUILayout.FloatField(_obj._spiralSize, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        if (existFlag)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("There is NavGrid, width: " + w + " and height: " + h);
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create NavMesh!"))
            _obj.GenerateNavGrid();
        if (existFlag)
        {
            if (GUILayout.Button("Delete NavMesh!"))
                _obj.DeleteNavMesh();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear all patrol routes!"))
            _obj.ClearPatrolRoutes();
        GUILayout.EndHorizontal();
    }
}
