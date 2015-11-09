using UnityEngine;
using System.Collections;


public class SimpleCharacterMovement : MonoBehaviour {

    public float m_speed = 1.0f;
    public float m_rotationSpeed = 1.0f;

    private NavGridScript m_navGrid;
    private NavPath m_pathScript;
    private MovementEvalFuncDelegate m_del;

    private bool m_on_going_movement = false;
    private int m_step = 0;
    private float m_distanceOnStep = .0f;

    void Start ()
    {
        m_pathScript = new NavPath();
        m_navGrid = GameObject.FindObjectOfType<NavGridScript>();
        m_pathScript.Initialize(m_navGrid);
        m_pathScript.m_characterSize = .5f;
        m_del = new MovementEvalFuncDelegate(EvalPathMovementFunc);

        float x = transform.position.x;
        float y = transform.position.y;
        if (!m_navGrid.IsWorldPositionAccessable(ref x, ref y))
            Debug.LogError("Character " + gameObject.name + "is in unaccessable location");

        transform.position = new Vector3(x, y, transform.position.z);
    }

    public void MoveTo(Vector3 to)
    {
        if (m_on_going_movement)
            return;

        m_distanceOnStep = .0f;
        m_step = 0;
        m_on_going_movement = true;
        m_pathScript.SeekPath(m_del, transform.position, to - new Vector3(.5f, .5f, .5f));
        if (m_pathScript.EditPathToFitCatmullRomSpline())
        {
            CatmullRomSpline spline = new CatmullRomSpline(m_pathScript.m_path);               
            StartCoroutine(InterpolateCurveMovementCoroutine(spline));
        }
        else
        {
            StartCoroutine(InterpolateTwoPointsLerpMovementCoroutine(m_pathScript.m_startWorldPos, m_pathScript.m_endWorldPos, true));
        }
    }

    float EvalPathMovementFunc(NavMovementEvalData d) { return d.f; }

    IEnumerator InterpolateCurveMovementCoroutine(CatmullRomSpline spline)
    {
        while (m_on_going_movement)
        {
            float d = m_speed * Time.deltaTime;
            m_distanceOnStep += d;
            while (m_distanceOnStep > m_navGrid.m_currentCellSize)
            {
                m_distanceOnStep -= m_navGrid.m_currentCellSize;
                spline.NextSection();
                m_step++;
            }

            if (m_step < (m_pathScript.m_path.Count - 4))
            {
                Vector3 p = spline.Interpolate(m_distanceOnStep / m_navGrid.m_currentCellSize);
                Quaternion look = Quaternion.LookRotation(Vector3.forward, (spline.NextPoint() - transform.position).normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * m_rotationSpeed);
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
        while (m_on_going_movement)
        {
            Quaternion look = Quaternion.LookRotation(Vector3.forward, (end - start).normalized);
            float dist = Vector3.Distance(start, end);
            float d = m_speed * Time.deltaTime;
            if (rotation)
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * m_rotationSpeed);
            m_distanceOnStep += d;
            if (m_distanceOnStep >= dist)
            {
                transform.position = end;
                m_on_going_movement = false;
                yield break;
            }
            else
            {
                transform.position = Vector3.Lerp(start, end, m_distanceOnStep / dist);
            }
            yield return null;
        }
     }

 }
