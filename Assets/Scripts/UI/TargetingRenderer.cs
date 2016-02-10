using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetingRenderer : MonoBehaviour {

	private Mesh m_mesh;
	private MeshRenderer m_renderer;
	private ActionManager m_actionManager;

	List<Vector3> m_vertices = new List<Vector3>();
	List<Vector3> m_normals = new List<Vector3>();
	List<Vector2> m_uvs = new List<Vector2>();
	List<int> m_indices = new List<int>();

	// Use this for initialization
	void Start () {
		m_mesh = GetComponent<MeshFilter>().mesh;
		m_renderer = GetComponent<MeshRenderer>();
		m_actionManager = FindObjectOfType<ActionManager>();
		m_mesh.name = "Targeting line";
		m_mesh.MarkDynamic();
	}
	
	// Update is called once per frame
	void Update () {

		var player = CharManager.GetLocalPlayer();
		if (!m_actionManager.m_currentlyTargeting || player == null) // If not targeting right now, disable renderer and exit function
		{
			m_renderer.enabled = false;
			return;
		}

		m_renderer.enabled = true;

		m_mesh.Clear();
		m_vertices.Clear();
		m_normals.Clear();
		m_uvs.Clear();
		m_indices.Clear();

		var startPoint = MapGrid.GridToWorldPoint(player.m_mover.m_gridPos) + new Vector3(0,0,-1);
		var endPoint = MapGrid.GridToWorldPoint(InputHandler.GetMouseGridPosition()) + new Vector3(0, 0, -1);
		var normal = new Vector3(0, 0, -1);
		var biNormal = Vector3.Cross(normal, endPoint - startPoint).normalized * .25f;

		float length = (endPoint - startPoint).magnitude;

		AddVertex(startPoint + biNormal, normal, new Vector2(0, 1));
		AddVertex(startPoint - biNormal, normal, new Vector2(0, 0));
		AddVertex(endPoint + biNormal, normal, new Vector2(length, 1));

		AddVertex(endPoint - biNormal, normal, new Vector2(length, 0));
		AddVertex(endPoint + biNormal, normal, new Vector2(length, 1));
		AddVertex(startPoint - biNormal, normal, new Vector2(0, 0));

		m_mesh.vertices = m_vertices.ToArray();
		m_mesh.normals = m_normals.ToArray();
		m_mesh.uv = m_uvs.ToArray();
		m_mesh.triangles = m_indices.ToArray();
	}

	void AddVertex(Vector3 p, Vector3 n, Vector2 t)
	{
		m_vertices.Add(p);
		m_normals.Add(n);
		m_uvs.Add(t);
		m_indices.Add(m_vertices.Count - 1);
	}
}
