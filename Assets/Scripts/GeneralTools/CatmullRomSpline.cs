using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatmullRomSpline {
	
    private List<Vector3> m_pts;
    private int m_currentSection = 0;
    public int Count
    {
        get { return m_pts.Count; }
    }


    public CatmullRomSpline(List<Vector3> points)
    {
        m_pts = points;
	}

	public Vector3 Interpolate(float t) {

        Vector3 a = m_pts[m_currentSection];
        Vector3 b = m_pts[m_currentSection + 1];
        Vector3 c = m_pts[m_currentSection + 2];
        Vector3 d = m_pts[m_currentSection + 3];

		return .5f * (
			(-a + 3f * b - 3f * c + d) * (t * t * t)
			+ (2f * a - 5f * b + 4f * c - d) * (t * t)
			+ (-a + c) * t
			+ 2f * b
		);
	}

    public void NextSection() 
    {
        m_currentSection++;
    }

    public Vector3 GetPoint(int index) { return m_pts[index];  }

    public Vector3 NextPoint(){ return m_pts[m_currentSection + 2]; }

    public Vector3 GetLastPoint() { return m_pts[m_pts.Count - 2];  }
    
    public static bool EditPathToFitCatmullRomSpline(ref List<Vector3> path)
    {
        if (path.Count >= 2)
        {
            Vector3 lastControlPoint = path[path.Count - 1];
            Vector3 secondLastControlPoint = path[path.Count - 2];
            Vector3 p = lastControlPoint + (lastControlPoint - secondLastControlPoint);
            path.Add(p);

            Vector3 firstControlPoint = path[0];
            Vector3 secondControlPoint = path[1];
            p = firstControlPoint + (firstControlPoint - secondControlPoint);
            path.Insert(0, p);
            return true;
        }
        else
        {
            return false;
        }
    }
}
