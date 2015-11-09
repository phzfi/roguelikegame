using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatrolRoute : MonoBehaviour {
    
    public List<Vector3> m_pointList = new List<Vector3>();
    public bool m_drawGizmos = true;
    private float m_d = .0f;
    private int m_currentPt = 0;
    private int m_nextPt = 1;

    public Vector3 GetNextPosition(float dist)
    {
        m_d += dist;
        float ptDist = Vector3.Distance(m_pointList[m_currentPt], m_pointList[m_nextPt]);
        if (m_d > ptDist)
        {
            m_d -= ptDist;
            m_currentPt++;
            m_currentPt = m_currentPt % m_pointList.Count;
            m_nextPt = (m_currentPt + 1) % m_pointList.Count;
            ptDist = Vector3.Distance(m_pointList[m_currentPt], m_pointList[m_nextPt]);
        }
        return Vector3.Lerp(m_pointList[m_currentPt], m_pointList[m_nextPt], m_d / ptDist);
    }

    public Vector3 GetDirection()
    {
        return (m_pointList[m_nextPt] - m_pointList[m_currentPt]).normalized;
    }

    public void Reset() { m_d = .0f; m_currentPt = 0; m_nextPt = 1; } 

    void OnDrawGizmos()
    {
        if (!m_drawGizmos)
            return;
        Gizmos.color = Color.blue;
        for (int i = 0; i < m_pointList.Count; ++i)
        {
            Gizmos.DrawSphere(m_pointList[i], .25f);
        }
        if (m_pointList.Count >= 2)
        {
            for (int i = 0; i < m_pointList.Count; ++i)
            {
                int next = (i + 1) % m_pointList.Count;
                Gizmos.DrawLine(m_pointList[i], m_pointList[next]);
            }
        }
    }
}
