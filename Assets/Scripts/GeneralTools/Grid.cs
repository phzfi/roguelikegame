using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
    public float _width = 1.0f;
    public float _height = 1.0f;
    public GameObject _prefab;
    public GameObject _enemyPrefab;

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
        for (float y = pos.y - 800.0f; y < pos.y + 800.0f; y += _height)
        {
            Gizmos.DrawLine(new Vector3(-1000000.0f, Mathf.Floor(y / _height) * _height, 0.0f),
                            new Vector3(1000000.0f, Mathf.Floor(y / _height) * _height, 0.0f));
        }

        for (float x = pos.x - 1200.0f; x < pos.x + 1200.0f; x += _width)
        {
            Gizmos.DrawLine(new Vector3(Mathf.Floor(x / _width) * _width, -1000000.0f, 0.0f),
                            new Vector3(Mathf.Floor(x / _width) * _width, 1000000.0f, 0.0f));
        }
    }
}