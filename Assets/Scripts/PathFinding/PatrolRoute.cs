using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatrolRoute : MonoBehaviour {
    
    public List<Vector3> _pointList = new List<Vector3>();
    public bool _drawGizmos = true;
    private float _d = .0f;
    private int _currentPt = 0;
    private int _nextPt = 1;

    public Vector3 GetNextPosition(float dist)
    {
        _d += dist;
        float ptDist = Vector3.Distance(_pointList[_currentPt], _pointList[_nextPt]);
        if (_d > ptDist)
        {
            _d -= ptDist;
            _currentPt++;
            _currentPt = _currentPt % _pointList.Count;
            _nextPt = (_currentPt + 1) % _pointList.Count;
            ptDist = Vector3.Distance(_pointList[_currentPt], _pointList[_nextPt]);
        }
        return Vector3.Lerp(_pointList[_currentPt], _pointList[_nextPt], _d / ptDist);
    }

    public Vector3 GetDirection()
    {
        return (_pointList[_nextPt] - _pointList[_currentPt]).normalized;
    }

    public void Reset() { _d = .0f; _currentPt = 0; _nextPt = 1; } 

    void OnDrawGizmos()
    {
        if (!_drawGizmos)
            return;
        Gizmos.color = Color.blue;
        for (int i = 0; i < _pointList.Count; ++i)
        {
            Gizmos.DrawSphere(_pointList[i], .25f);
        }
        if (_pointList.Count >= 2)
        {
            for (int i = 0; i < _pointList.Count; ++i)
            {
                int next = (i + 1) % _pointList.Count;
                Gizmos.DrawLine(_pointList[i], _pointList[next]);
            }
        }
    }
}
