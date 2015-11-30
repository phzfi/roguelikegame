using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatmullRomSpline
{
	public int ControlPointCount { get { return m_controlPoints.Count; } }
	public int SectionCount { get { return Mathf.Max(m_controlPoints.Count - 3, 0); } }

	private List<Vector3> m_controlPoints = null;
	private int m_currentSection = 0;

	public CatmullRomSpline(List<Vector3> controlPoints)
	{
		Debug.Assert(controlPoints.Count >= 2, "Trying to create Catmull Rom Spline with less than two points!");

		m_controlPoints = controlPoints;

		Vector3 lastControlPoint = m_controlPoints[m_controlPoints.Count - 1];
		Vector3 secondLastControlPoint = m_controlPoints[m_controlPoints.Count - 2];
		Vector3 p = lastControlPoint + (lastControlPoint - secondLastControlPoint);
		m_controlPoints.Add(p);

		Vector3 firstControlPoint = m_controlPoints[0];
		Vector3 secondControlPoint = m_controlPoints[1];
		p = firstControlPoint + (firstControlPoint - secondControlPoint);
		m_controlPoints.Insert(0, p);
	}
	
	public void StartInterpolation()
	{
		m_currentSection = 0;
	}

	public float GetSectionLength()
	{
		return GetSectionLength(m_currentSection);
    }

	public float GetSectionLength(int section)
	{
		Vector3 b = m_controlPoints[section + 1];
		Vector3 c = m_controlPoints[section + 2];
		return Vector3.Distance(b, c);
	}

	public Vector3 Interpolate(float t)
	{
		return Interpolate(t, m_currentSection);
    }

	public Vector3 Interpolate(float t, int section)
	{
		Debug.Assert(section >= 0 && section + 3 < m_controlPoints.Count, "Current section out of bounds!");

		Vector3 a = m_controlPoints[section];
		Vector3 b = m_controlPoints[section + 1];
		Vector3 c = m_controlPoints[section + 2];
		Vector3 d = m_controlPoints[section + 3];

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

	public Vector3 GetControlPoint(int index)
	{
		Debug.Assert(index >= 0 && index < m_controlPoints.Count, "Index out of bounds!");
		return m_controlPoints[index];
	}

	public Vector3 GetNextControlPoint()
	{
		return m_controlPoints[m_currentSection + 2];
	}

	public Vector3 GetLastControlPoint()
	{
		return m_controlPoints[m_controlPoints.Count - 2];
	}
}
