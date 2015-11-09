using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(NavGridScript))]
public class NavGridEditor : Editor {

    private NavGridScript m_obj;
    private bool existFlag = false;

    public void OnEnable()
    {
        m_obj = (NavGridScript)target;
    }

    public override void OnInspectorGUI()
    {
        int w; int h;
        existFlag = m_obj.IsGridGenerated(out w, out h);

        GUILayout.BeginHorizontal();
        GUILayout.Label("NavGrid width");
        m_obj.m_width = EditorGUILayout.FloatField(m_obj.m_width, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("NavGrid height");
        m_obj.m_height = EditorGUILayout.FloatField(m_obj.m_height, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("NavGrid Cell size");
        m_obj.m_cellSize = EditorGUILayout.FloatField(m_obj.m_cellSize, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("NavGrid Spiral size");
        m_obj.m_spiralSize = EditorGUILayout.FloatField(m_obj.m_spiralSize, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        if (existFlag)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("There is NavGrid, width: " + w + " and height: " + h);
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create NavMesh!"))
            m_obj.GenerateNavGrid();
        if (existFlag)
        {
            if (GUILayout.Button("Delete NavMesh!"))
                m_obj.DeleteNavMesh();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear all patrol routes!"))
            m_obj.ClearPatrolRoutes();
        GUILayout.EndHorizontal();
    }
}
