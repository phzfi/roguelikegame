using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleCharacterMovement : MonoBehaviour {
    
    public int ID;

    public NavPath m_pathScript;

    public Vector2i m_gridPos;
    public Vector3 m_worldPos;
    public PlayerSync m_syncer;
    private NavGridScript m_navGrid;
    private MovementEvalFuncDelegate m_del;
    private AudioSource m_audioSource;

    bool m_onGoingMovement = false;
    float m_distanceOnStep = .0f;
    float m_visualizationSpeed = 4.0f;
    float m_visualizationRotationSpeed = 6.0f;
    int m_gridSpeed = 6;
    int m_step = 0;

    void Start ()
    {
        m_navGrid = NavGridScript.Instance;

        m_pathScript = new NavPath();
        m_pathScript.Initialize(m_navGrid);
        m_pathScript.m_characterSize = .5f;
        m_audioSource = GetComponent<AudioSource>();
        m_syncer = GetComponent<PlayerSync>();

        m_del = new MovementEvalFuncDelegate(EvalPathMovementFunc);

        float x = transform.position.x;
        float y = transform.position.y;
        if (!NavGridScript.Instance.IsWorldPositionAccessable(ref x, ref y))
            Debug.LogError("Character " + gameObject.name + ", ID: " + ID + " is in unaccessable location");

        transform.position = new Vector3(x, y, transform.position.z);

        MovementManager.Register(this, out ID);
    }
    float EvalPathMovementFunc(NavMovementEvalData d) { return d.f; }
    
    public void OnDestroy()
    {
        MovementManager.Unregister(ID);
    }

    public void MoveCommand(Vector3 to)
    {
        m_pathScript.m_path = m_pathScript.SeekPath(m_del, transform.position, to - new Vector3(.5f, .5f, .5f));
    }

    public void VisualizeMove(Vector3 to)
    {
        List<Vector3> tempPath = m_pathScript.SeekPath(m_del, transform.position, to - new Vector3(.5f, .5f, .5f));
        if (tempPath.Count == 0)
            return;

        List<Vector3> currentPath = new List<Vector3>();
        for (int i = 0; i < m_gridSpeed; ++i)
        {
            if (i == tempPath.Count)
                break;
            currentPath.Add(tempPath[i]);
        }

        m_distanceOnStep = .0f;
        m_step = 0;
        m_onGoingMovement = true;

        StopAllCoroutines(); // kill previous interpolations if they're still going

        if (CatmullRomSpline.EditPathToFitCatmullRomSpline(ref currentPath))
        {
            CatmullRomSpline spline = new CatmullRomSpline(currentPath);
            StartCoroutine(InterpolateCurveMovementCoroutine(spline, currentPath));
        }
        else
        {
            StartCoroutine(InterpolateTwoPointsLerpMovementCoroutine(m_pathScript.m_startWorldPos, m_pathScript.m_endWorldPos, true));
        }
    }

    IEnumerator InterpolateCurveMovementCoroutine(CatmullRomSpline spline, List<Vector3> currentPath)
    {
        while (m_onGoingMovement)
        {
            float d = m_visualizationSpeed * Time.deltaTime;
            m_distanceOnStep += d;
            while (m_distanceOnStep > NavGridScript.Instance.m_currentCellSize)
            {
                m_distanceOnStep -= NavGridScript.Instance.m_currentCellSize;
                spline.NextSection();
                m_step++;
            }

            if (m_step < (currentPath.Count - 4))
            {
                Vector3 p = spline.Interpolate(m_distanceOnStep / NavGridScript.Instance.m_currentCellSize);
                Quaternion look = Quaternion.LookRotation(Vector3.forward, (spline.NextPoint() - transform.position).normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * m_visualizationRotationSpeed);
                transform.position = p;
            }
            else
            {
                StartCoroutine(InterpolateTwoPointsLerpMovementCoroutine(transform.position, spline.GetLastPoint(), true));
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator InterpolateTwoPointsLerpMovementCoroutine(Vector3 start, Vector3 end, bool rotation)
    {
        while (m_onGoingMovement)
        {
            Quaternion look = Quaternion.LookRotation(Vector3.forward, (end - start).normalized);
            float dist = Vector3.Distance(start, end);
            float d = m_visualizationSpeed * Time.deltaTime;
            if (rotation)
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * m_visualizationRotationSpeed);
            m_distanceOnStep += d;
            if (m_distanceOnStep >= dist)
            {
                transform.position = end;
                m_onGoingMovement = false;
                yield break;
            }
            else
            {
                transform.position = Vector3.Lerp(start, end, m_distanceOnStep / dist);
            }
            yield return null;
        }
    }

    public bool TakeStep()
    {
        if (m_pathScript.m_path.Count == 0)
            return false;

        bool moved = false;
        Debug.Log("Taking step");

        for (int step = 0; step < m_gridSpeed; step++)
        {
            if (m_pathScript.m_path.Count == 0)
                return moved;

            var nextPos = m_pathScript.m_path[0];

            bool movementBlocked = false;
            for (int i = 0; i < MovementManager.Objects.Count; ++i) // loop over objects to check next path step is not blocked
            {
                var mover = MovementManager.Objects[i];
                if (mover == this)
                    continue;

                if (mover.m_gridPos == m_pathScript.GetGridPosition(nextPos))
                {
                    // TODO: do something smart here, don't just stop?
                    movementBlocked = true;
                    break;
                }
            }

            if (!movementBlocked) // if nextPos was not blocked, move there and remove one segment from path
            {
                m_gridPos = m_pathScript.GetGridPosition(nextPos);
                m_worldPos = nextPos;
                m_pathScript.m_path.RemoveAt(0);
                moved = true;

                for(int i = 0; i < ItemManager.ItemsOnMap.Count; ++i)
                {
                    var item = ItemManager.ItemsOnMap[i];
                    if(item.m_pos == m_gridPos && item.CanPickup(gameObject))
                        SyncManager.AddPickupOrder(ID, item.ID);
                }
            }
            else
                return moved;
        }

        m_audioSource.Play();
        return moved;
    }

 }
