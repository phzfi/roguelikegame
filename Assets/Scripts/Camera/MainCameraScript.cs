using UnityEngine;
using System.Collections;

public class MainCameraScript : MonoBehaviour {

    public float m_camera_acceleration_constant = .1f;
    public float m_camera_max_velocity = 1.0f;

    private Vector3 m_max_velocity = new Vector3();
    private Vector3 m_velocity;

    void Awake()
    {
        m_max_velocity = new Vector3(
            m_camera_max_velocity,
            m_camera_max_velocity,
            m_camera_max_velocity);
    }

	void Update ()
    {
        var input = InputHandler.Instance.CameraInput;
        var dt = Time.deltaTime;
        m_velocity = Vector3.Lerp(m_velocity, 
            MultiplyComponentsOfVectors(input, m_max_velocity), m_camera_acceleration_constant * dt);
        transform.localPosition += transform.localRotation * m_velocity * dt;
    }

    private Vector3 MultiplyComponentsOfVectors(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}
