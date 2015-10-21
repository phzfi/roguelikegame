using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour
{
	public AnimationCurve m_curve;

	private Vector3 m_vStartPos;

	void Start()
	{
		m_vStartPos = transform.localPosition;
	}

	void Update()
	{
        Vector3 vPos = transform.localPosition;
		vPos.y = m_vStartPos.y + m_curve.Evaluate(Time.time);
		transform.localPosition = vPos;
	}
}
