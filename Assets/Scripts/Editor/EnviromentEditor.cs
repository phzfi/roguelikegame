using UnityEngine;
using UnityEditor;
using System.Collections;

public class EnviromentEditor : EditorWindow {

    enum EnvEditorState
    {
        EnvEditorState_Boundary,
        EnvEditorState_PatrolAndSpawn
    }

	public string _sceneName;
	public float _sceneWidth;
	public float _sceneHeight;
    public float _sceneDepth;
	public GameObject _prefab;
    public GameObject _enemyPrefab;
    public float _depth;

    private GameObject _overlayObj;
    private GameObject _gridObj;
    private GameObject _currentObj;
    private GameObject _navOverlayObj;
    private Grid _grid;
    private Vector3 _firstMousePos;
    private EnvEditorState _state;
    private bool _editorInit = false;
    private int _patrolRouteCounter = 0;
    private int _spawnCounter = 0;

    [MenuItem("Window/EnviromentEditor")]
	public static void ShowWindow()
	{
        EditorWindow.GetWindow(typeof(EnviromentEditor));
	}

	void OnGUI()
	{
        if (!_editorInit)
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            _sceneName = EditorGUILayout.TextField("Name", _sceneName);
            _sceneWidth = EditorGUILayout.FloatField("Width of the scene", _sceneWidth);
            _sceneHeight = EditorGUILayout.FloatField("Height of the scene", _sceneHeight);
            _sceneDepth = EditorGUILayout.FloatField("Depth of the scene", _sceneDepth);

            GUILayout.Label("Boundary Prefab", EditorStyles.miniLabel);
            _prefab = (GameObject)EditorGUILayout.ObjectField(_prefab, typeof(Object), true);
            if (GUILayout.Button("Create!"))
            {
                if (_prefab == null)
                    ShowNotification(new GUIContent("No boundary prefab selected"));
                else
                {
                    SceneBoundaries creator = new SceneBoundaries();
                    _overlayObj = new GameObject("En_BoundariesOverlay - " + _sceneName);
                    _overlayObj.AddComponent<EnviromentOverlayScript>();
                    creator.CreateBoundaries(new Vector3(_sceneWidth, _sceneHeight, _sceneDepth), _prefab, _overlayObj);
                    _gridObj = new GameObject("GridEditor");
                    _gridObj.AddComponent<Grid>();
                    _grid = _gridObj.GetComponent<Grid>();
                    _grid._prefab = _prefab;
                    _grid._enemyPrefab = _enemyPrefab;
                    _navOverlayObj = GameObject.Find("Nav_Overlay");
                    _state = EnvEditorState.EnvEditorState_Boundary;
                    SceneView.onSceneGUIDelegate = BoundaryUpdate;
                    _depth = _sceneDepth;
                    _editorInit = true;
                }
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" Editor State ");
            EnvEditorState s;
            s = (EnvEditorState)EditorGUILayout.EnumPopup(_state);
            if (s != _state)
            {
                if (s == EnvEditorState.EnvEditorState_Boundary)
                    SceneView.onSceneGUIDelegate = BoundaryUpdate;
                else if (s == EnvEditorState.EnvEditorState_PatrolAndSpawn)
                    SceneView.onSceneGUIDelegate = PatrolUpdate;
                _state = s;
                _currentObj = null;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" Grid Width ");
            _grid._width = EditorGUILayout.Slider(_grid._width, .1f, 5.0f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(" Grid Height ");
            _grid._height = EditorGUILayout.Slider(_grid._height, .1f, 5.0f);
            GUILayout.EndHorizontal();


            if (_state == EnvEditorState.EnvEditorState_Boundary)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(" Depth ");
                _depth = EditorGUILayout.FloatField(_depth, GUILayout.Width(50));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(" Object Prefab ");
                _grid._prefab = (GameObject)EditorGUILayout.ObjectField(_grid._prefab, typeof(GameObject), true);
                GUILayout.EndHorizontal();
            }
            else if (_state == EnvEditorState.EnvEditorState_PatrolAndSpawn)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label("Reset counters: ", EditorStyles.miniLabel);
                GUILayout.Label(_spawnCounter.ToString());
                if (GUILayout.Button("Spawn"))
                {
                    _spawnCounter = 0;
                }
                GUILayout.Label(_patrolRouteCounter.ToString());
                if (GUILayout.Button("Patrol"))
                {
                    _patrolRouteCounter = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Enemy Prefab", EditorStyles.miniLabel);
                _grid._enemyPrefab = (GameObject)EditorGUILayout.ObjectField(_grid._enemyPrefab, typeof(Object), true);
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Exit EnviromentEditor!"))
            {
                _enemyPrefab = _grid._enemyPrefab;
                _prefab = _grid._prefab;
                if (_gridObj != null)
                    GameObject.DestroyImmediate(_gridObj);
                _editorInit = false;
            }
        }
	}

    void BoundaryUpdate(SceneView sceneview)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && e.control)
        {
            if (_grid._prefab)
            {
                Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(r, out hit, 1000.0f))
                {
                    _firstMousePos = new Vector3(Mathf.Floor(hit.point.x / _grid._width) * _grid._width + _grid._width / 2.0f,
                                Mathf.Floor(hit.point.y / _grid._height) * _grid._height + _grid._height / 2.0f, hit.point.z - .5f * _depth);
                    _currentObj = Instantiate(_grid._prefab, _firstMousePos, Quaternion.identity) as GameObject;
                    _currentObj.transform.parent = _overlayObj.transform;
                    _currentObj.transform.localScale = new Vector3(_currentObj.transform.localScale.x, _currentObj.transform.localScale.y, _depth);
                }
            }
            else
                Debug.Log("No object prefab selected");
        }
        else if (e.alt && _currentObj != null)
        {
            Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(r, out hit, 1000.0f))
            {
                Vector3 aligned = new Vector3(Mathf.Floor(hit.point.x / _grid._width) * _grid._width + _grid._width / 2.0f,
                            Mathf.Floor(hit.point.y / _grid._height) * _grid._height + _grid._height / 2.0f, hit.point.z - .5f * _depth);
                Vector3 diff = new Vector3(aligned.x - _firstMousePos.x, aligned.y - _firstMousePos.y, .0f);
                if (diff.magnitude != .0f)
                {
                    if (_currentObj != null)
                    {
                        _currentObj.transform.localScale = new Vector3(Mathf.Abs(diff.x) + 1.0f, Mathf.Abs(diff.y) + 1.0f, _currentObj.transform.localScale.z);
                        _currentObj.transform.localPosition = diff * .5f + _firstMousePos;
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
                Vector3 p = new Vector3(Mathf.Floor(hit.point.x / _grid._width) * _grid._width + _grid._width / 2.0f,
                            Mathf.Floor(hit.point.y / _grid._height) * _grid._height + _grid._height / 2.0f, hit.point.z);
                _currentObj = new GameObject("PatrolRoute" + _patrolRouteCounter);
                _currentObj.AddComponent<PatrolRoute>();
                _currentObj.GetComponent<PatrolRoute>()._pointList.Add(p);
                _currentObj.transform.position = p;
                _patrolRouteCounter++;
                _currentObj.transform.parent = _navOverlayObj.transform;
            }
        }
        else if (e.type == EventType.MouseDown && e.alt && _currentObj != null)
        {
            Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(r, out hit, 1000.0f))
            {
                Vector3 p = new Vector3(Mathf.Floor(hit.point.x / _grid._width) * _grid._width + _grid._width / 2.0f,
                            Mathf.Floor(hit.point.y / _grid._height) * _grid._height + _grid._height / 2.0f, hit.point.z);
                PatrolRoute route = _currentObj.GetComponent<PatrolRoute>();
                if (!route._pointList.Exists(d=>d == p))
                {
                    route._pointList.Add(p);
                }
                
            }
        }
        else if (e.type == EventType.MouseDown && e.shift)
        {
            GameObject overlay = GameObject.Find("Char_SpawnOverlay");
            if (overlay == null)
            {
                overlay = new GameObject("Char_SpawnOverlay");
            }
            Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(r, out hit, 1000.0f))
            {
                Vector3 p = new Vector3(Mathf.Floor(hit.point.x / _grid._width) * _grid._width + _grid._width / 2.0f,
                            Mathf.Floor(hit.point.y / _grid._height) * _grid._height + _grid._height / 2.0f, hit.point.z);
                string n = " - ";
                if(_grid._enemyPrefab != null)
                    n += _grid._enemyPrefab.name;
                GameObject obj = new GameObject("SpawnPoint" + _spawnCounter + n);
                _spawnCounter++;
                obj.transform.position = p;
                obj.transform.parent = overlay.transform;
                CharSpawnPointScript spawn = obj.AddComponent<CharSpawnPointScript>();
                spawn._prefab = _grid._enemyPrefab;
                if (_patrolRouteCounter > 0)
                    spawn._pathToPatrolRoute = "PatrolRoute" + (_patrolRouteCounter - 1); 
            }
        }
    }
}
