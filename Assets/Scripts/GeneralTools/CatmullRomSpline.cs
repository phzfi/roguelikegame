﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CatmullRomSpline {
	
    private List<Vector3> _pts;
    private int _currentSection = 0;
    public int Count
    {
        get { return _pts.Count; }
    }


    public CatmullRomSpline(List<Vector3> points)
    {
        _pts = points;
	}

	public Vector3 Interpolate(float t) {

        Vector3 a = _pts[_currentSection];
        Vector3 b = _pts[_currentSection + 1];
        Vector3 c = _pts[_currentSection + 2];
        Vector3 d = _pts[_currentSection + 3];

		return .5f * (
			(-a + 3f * b - 3f * c + d) * (t * t * t)
			+ (2f * a - 5f * b + 4f * c - d) * (t * t)
			+ (-a + c) * t
			+ 2f * b
		);
	}

    public void NextSection() 
    {
        _currentSection++;
    }

    public Vector3 GetPoint(int index) { return _pts[index];  }

    public Vector3 NextPoint(){ return _pts[_currentSection + 2]; }

    public Vector3 GetLastPoint() { return _pts[_pts.Count - 2];  }
	
}
