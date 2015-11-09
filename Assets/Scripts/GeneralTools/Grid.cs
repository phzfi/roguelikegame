using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
    public float m_width = 1.0f;
    public float m_height = 1.0f;
    public GameObject m_prefab;
    public GameObject m_enemyPrefab;

    void Start()
    {
    }

    void Update()
    {
    }

    void OnDrawGizmos()
    {
        Vector3 pos = Camera.current.transform.position;
        Gizmos.color = Color.black;
        for (float y = pos.y - 800.0f; y < pos.y + 800.0f; y += m_height)
        {
            Gizmos.DrawLine(new Vector3(-1000000.0f, Mathf.Floor(y / m_height) * m_height, 0.0f),
                            new Vector3(1000000.0f, Mathf.Floor(y / m_height) * m_height, 0.0f));
        }

        for (float x = pos.x - 1200.0f; x < pos.x + 1200.0f; x += m_width)
        {
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x / m_width) * m_width, -1000000.0f, 0.0f),
                            new Vector3(Mathf.Floor(x / m_width) * m_width, 1000000.0f, 0.0f));
        }
    }
}