using UnityEngine;
using System.Collections;

public class LevelMapManager : MonoBehaviour {

    private LevelMap m_map;
    public bool m_debug;

	void Start ()
    {
        //TODO Procedural generation of level, now just reads pregenerated NavGrid

        NavGrid nav = GameObject.FindObjectOfType<NavGridScript>().m_grid;
        m_map = new LevelMap(nav.Width, nav.Height);
        m_map.Generate(nav);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
