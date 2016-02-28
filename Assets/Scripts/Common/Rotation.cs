using UnityEngine;
using System.Collections;

public class Rotation : MonoBehaviour
{
	public Vector3 rotationSpeed = Vector3.zero;

	private Vector3 m_rotation;

	void OnEnable()
	{
		m_rotation = transform.localRotation.eulerAngles;
	}

	void Update()
	{
		if (!Mathf.Approximately(rotationSpeed.x, 0.0f))
		{
			m_rotation.x += rotationSpeed.x * Time.deltaTime;
		}

		if (!Mathf.Approximately(rotationSpeed.y, 0.0f))
		{
			m_rotation.y += rotationSpeed.y * Time.deltaTime;
		}

		if (!Mathf.Approximately(rotationSpeed.z, 0.0f))
		{
			m_rotation.z += rotationSpeed.z * Time.deltaTime;
		}

		transform.localRotation = Quaternion.Euler(m_rotation);
	}

}
