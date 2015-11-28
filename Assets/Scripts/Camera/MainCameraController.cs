using UnityEngine;
using System.Collections;

public class MainCameraController : MonoBehaviour
{
	public float m_speed = 3.0f;

	[SerializeField]
	private Transform m_target = null;

	void LateUpdate()
	{
		if (m_target)
		{
			Vector3 targetPosition = m_target.position;
			targetPosition.z = 0;
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * m_speed);
		}
	}

	public void SetTarget(Transform target, bool snapToTarget)
	{
		Debug.Assert(target, "Tring to set null target");

		m_target = target;

		if (snapToTarget)
		{
			Vector3 targetPosition = m_target.position;
			targetPosition.z = 0;
			transform.position = targetPosition;
		}
	}
}
