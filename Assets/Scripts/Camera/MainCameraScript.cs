using UnityEngine;
using System.Collections;

public class MainCameraScript : MonoBehaviour {

    public float m_cameraAccelerationConstant = .1f;
    public float m_cameraMaxVelocity = 1.0f;

    private Vector3 m_maxVelocity = new Vector3();
    private Vector3 m_velocity;

    void Awake()
    {
        m_maxVelocity = new Vector3(
            m_cameraMaxVelocity,
            m_cameraMaxVelocity,
            m_cameraMaxVelocity);
    }

	void Update ()
    {
        var input = InputHandler.Instance.CameraInput;
        var dt = Time.deltaTime;
        m_velocity = Vector3.Lerp(m_velocity, 
            MultiplyComponentsOfVectors(input, m_maxVelocity), m_cameraAccelerationConstant * dt);
        transform.localPosition += transform.localRotation * m_velocity * dt;
    }

    private Vector3 MultiplyComponentsOfVectors(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}
