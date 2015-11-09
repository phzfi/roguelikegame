using UnityEngine;
using UnityEditor;
using System.Collections;

public class EnviromentEditor : EditorWindow {

    enum EnvEditorState
    {
        EnvEditorStatem_Boundary,
        EnvEditorStatem_PatrolAndSpawn
    }

	public string m_sceneName;
	public float m_sceneWidth;
	public float m_sceneHeight;
    public float m_sceneDepth;
	public GameObject m_prefab;
    public GameObject m_enemyPrefab;
    public float m_depth;

    private GameObject m_overlayObj;
    private GameObject m_gridObj;
    private GameObject m_currentObj;
    private GameObject m_navOverlayObj;
    private Grid m_grid;
    private Vector3 m_firstMousePos;
    private EnvEditorState m_state;
    private bool m_editorInit = false;
    private int m_patrolRouteCounter = 0;
    private int m_spawnCounter = 0;

    [MenuItem("Window/EnviromentEditor")]
	public static void ShowWindow()
	{
        EditorWindow.GetWindow(typeof(EnviromentEditor));
	}

	void OnGUI()
	{
        if (!m_editorInit)
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            m_sceneName = EditorGUILayout.TextField("Name", m_sceneName);
            m_sceneWidth = EditorGUILayout.FloatField("Width of the scene", m_sceneWidth);
            m_sceneHeight = EditorGUILayout.FloatField("Height of the scene", m_sceneHeight);
            m_sceneDepth = EditorGUILayout.FloatField("Depth of the scene", m_sceneDepth);

            GUILayout.Label("Boundary Prefab", EditorStyles.miniLabel);
            m_prefab = (GameObject)EditorGUILayout.ObjectField(m_prefab, typeof(Object), true);
            if (GUILayout.Button("Create!"))
            {
                if (m_prefab == null)
                    ShowNotification(new GUIContent("No boundary prefab selected"));
                else
                {
                    SceneBoundaries creator = new SceneBoundaries();
                    m_overlayObj = new GameObject("Enm_BoundariesOverlay - " + m_sceneName);
                    m_overlayObj.AddComponent<EnviromentOverlayScript>();
                    creator.CreateBoundaries(new Vector3(m_sceneWidth, m_sceneHeight, m_sceneDepth), m_prefab, m_overlayObj);
                    m_gridObj = new GameObject("GridEditor");
                    m_gridObj.AddComponent<Grid>();
                    m_grid = m_gridObj.GetComponent<Grid>();
                    m_grid.m_prefab = m_prefab;
                    m_grid.m_enemyPrefab = m_enemyPrefab;
                    m_navOverlayObj = GameObject.Find("Navm_Overlay");
                    m_state = EnvEditorState.EnvEditorStatem_Boundary;
                    SceneView.onSceneGUIDelegate = BoundaryUpdate;
                    m_depth = m_sceneDepth;
                    m_editorInit = true;
                }
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" Editor State ");
            EnvEditorState s;
            s = (EnvEditorState)EditorGUILayout.EnumPopup(m_state);
            if (s != m_state)
            {
                if (s == EnvEditorState.EnvEditorStatem_Boundary)
                    SceneView.onSceneGUIDelegate = BoundaryUpdate;
                else if (s == EnvEditorState.EnvEditorStatem_PatrolAndSpawn)
                    SceneView.onSceneGUIDelegate = PatrolUpdate;
                m_state = s;
                m_currentObj = null;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" Grid Width ");
            m_grid.m_width = EditorGUILayout.Slider(m_grid.m_width, .1f, 5.0f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(" Grid Height ");
            m_grid.m_height = EditorGUILayout.Slider(m_grid.m_height, .1f, 5.0f);
            GUILayout.EndHorizontal();


            if (m_state == EnvEditorState.EnvEditorStatem_Boundary)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(" Depth ");
                m_depth = EditorGUILayout.FloatField(m_depth, GUILayout.Width(50));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(" Object Prefab ");
                m_grid.m_prefab = (GameObject)EditorGUILayout.ObjectField(m_grid.m_prefab, typeof(GameObject), true);
                GUILayout.EndHorizontal();
            }
            else if (m_state == EnvEditorState.EnvEditorStatem_PatrolAndSpawn)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label("Reset counters: ", EditorStyles.miniLabel);
                GUILayout.Label(m_spawnCounter.ToString());
                if (GUILayout.Button("Spawn"))
                {
                    m_spawnCounter = 0;
                }
                GUILayout.Label(m_patrolRouteCounter.ToString());
                if (GUILayout.Button("Patrol"))
                {
                    m_patrolRouteCounter = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Enemy Prefab", EditorStyles.miniLabel);
                m_grid.m_enemyPrefab = (GameObject)EditorGUILayout.ObjectField(m_grid.m_enemyPrefab, typeof(Object), true);
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Exit EnviromentEditor!"))
            {
                m_enemyPrefab = m_grid.m_enemyPrefab;
                m_prefab = m_grid.m_prefab;
                if (m_gridObj != null)
                    GameObject.DestroyImmediate(m_gridObj);
                m_editorInit = false;
            }
        }
	}

    void BoundaryUpdate(SceneView sceneview)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && e.control)
        {
            if (m_grid.m_prefab)
            {
                Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(r, out hit, 1000.0f))
                {
                    m_firstMousePos = new Vector3(Mathf.Floor(hit.point.x / m_grid.m_width) * m_grid.m_width + m_grid.m_width / 2.0f,
                                Mathf.Floor(hit.point.y / m_grid.m_height) * m_grid.m_height + m_grid.m_height / 2.0f, hit.point.z - .5f * m_depth);
                    m_currentObj = Instantiate(m_grid.m_prefab, m_firstMousePos, Quaternion.identity) as GameObject;
                    m_currentObj.transform.parent = m_overlayObj.transform;
                    m_currentObj.transform.localScale = new Vector3(m_currentObj.transform.localScale.x, m_currentObj.transform.localScale.y, m_depth);
                }
            }
            else
                Debug.Log("No object prefab selected");
        }
        else if (e.alt && m_currentObj != null)
        {
            Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(r, out hit, 1000.0f))
            {
                Vector3 aligned = new Vector3(Mathf.Floor(hit.point.x / m_grid.m_width) * m_grid.m_width + m_grid.m_width / 2.0f,
                            Mathf.Floor(hit.point.y / m_grid.m_height) * m_grid.m_height + m_grid.m_height / 2.0f, hit.point.z - .5f * m_depth);
                Vector3 diff = new Vector3(aligned.x - m_firstMousePos.x, aligned.y - m_firstMousePos.y, .0f);
                if (diff.magnitude != .0f)
                {
                    if (m_currentObj != null)
                    {
                        m_currentObj.transform.localScale = new Vector3(Mathf.Abs(diff.x) + 1.0f, Mathf.Abs(diff.y) + 1.0f, m_currentObj.transform.localScale.z);
                        m_currentObj.transform.localPosition = diff * .5f + m_firstMousePos;
                    }
                }
            }
        }
    }
    
    void PatrolUpdate(SceneView sceneview)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.control)
        {
            Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(r, out hit, 1000.0f))
            {
                Vector3 p = new Vector3(Mathf.Floor(hit.point.x / m_grid.m_width) * m_grid.m_width + m_grid.m_width / 2.0f,
                            Mathf.Floor(hit.point.y / m_grid.m_height) * m_grid.m_height + m_grid.m_height / 2.0f, hit.point.z);
                m_currentObj = new GameObject("PatrolRoute" + m_patrolRouteCounter);
                m_currentObj.AddComponent<PatrolRoute>();
                m_currentObj.GetComponent<PatrolRoute>().m_pointList.Add(p);
                m_currentObj.transform.position = p;
                m_patrolRouteCounter++;
                m_currentObj.transform.parent = m_navOverlayObj.transform;
            }
        }
        else if (e.type == EventType.MouseDown && e.alt && m_currentObj != null)
        {
            Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(r, out hit, 1000.0f))
            {
                Vector3 p = new Vector3(Mathf.Floor(hit.point.x / m_grid.m_width) * m_grid.m_width + m_grid.m_width / 2.0f,
                            Mathf.Floor(hit.point.y / m_grid.m_height) * m_grid.m_height + m_grid.m_height / 2.0f, hit.point.z);
                PatrolRoute route = m_currentObj.GetComponent<PatrolRoute>();
                if (!route.m_pointList.Exists(d=>d == p))
                {
                    route.m_pointList.Add(p);
                }
                
            }
        }
        else if (e.type == EventType.MouseDown && e.shift)
        {
            GameObject overlay = GameObject.Find("Charm_SpawnOverlay");
            if (overlay == null)
            {
                overlay = new GameObject("Charm_SpawnOverlay");
            }
            Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(r, out hit, 1000.0f))
            {
                Vector3 p = new Vector3(Mathf.Floor(hit.point.x / m_grid.m_width) * m_grid.m_width + m_grid.m_width / 2.0f,
                            Mathf.Floor(hit.point.y / m_grid.m_height) * m_grid.m_height + m_grid.m_height / 2.0f, hit.point.z);
                string n = " - ";
                if(m_grid.m_enemyPrefab != null)
                    n += m_grid.m_enemyPrefab.name;
                GameObject obj = new GameObject("SpawnPoint" + m_spawnCounter + n);
                m_spawnCounter++;
                obj.transform.position = p;
                obj.transform.parent = overlay.transform;
                CharSpawnPointScript spawn = obj.AddComponent<CharSpawnPointScript>();
                spawn.m_prefab = m_grid.m_enemyPrefab;
                if (m_patrolRouteCounter > 0)
                    spawn.m_pathToPatrolRoute = "PatrolRoute" + (m_patrolRouteCounter - 1); 
            }
        }
    }
}
