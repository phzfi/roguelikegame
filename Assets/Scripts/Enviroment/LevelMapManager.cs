using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

/*
Client side map information
*/

public class LevelMapManager : MonoBehaviour
{
    private LevelMapVisualization m_mapVisualization;
    private LevelMap m_map = null;

    void Start()
    {
        m_mapVisualization = FindObjectOfType<LevelMapVisualization>();
    }

    public void Initialize(int width, int height)
    {
        m_map = new LevelMap(false);
        m_map.GenerateEmpty(width, height);
        m_mapVisualization.Init(m_map);
    }

    public void AddToMap(Vector2i pos, MapTileType type, int visualizationIndex)
    {
        m_map.AddToMap(pos, type, visualizationIndex);
        m_map.GetNavGrid().SetCellAccessable(pos, m_map.IsAccessible(pos.x, pos.y));
        //m_mapVisualization.MeshSingle(m_map, pos.x, pos.y);
    }
    
    public void Visualize()
    {
        m_mapVisualization.MarchingSquaresMesh(m_map);
    } 

    public void Reset()
    {
        m_map = null;
        //TODO Reset visualization
    }

	public LevelMap GetMap()
	{
		Debug.Assert(m_map == null, "Trying to use map before it's created");
		return m_map;
	}

}
