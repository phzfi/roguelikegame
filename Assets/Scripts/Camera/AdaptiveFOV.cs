using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class AdaptiveFOV : MonoBehaviour
{
	public float m_fov = 90.0f;

	private Camera m_camera = null;

	void Awake()
	{
		m_camera = GetComponent<Camera>();
	}

	void LateUpdate()
	{
		// Clamp aspect ratio so that wider aspects don't provide more information on the world
		const float defaultAspectRatio = 16.0f / 9.0f;
		float aspectRatio = Mathf.Max((float)Screen.width / (float)Screen.height, defaultAspectRatio);
		m_camera.fieldOfView = m_fov / aspectRatio;
	}
}
