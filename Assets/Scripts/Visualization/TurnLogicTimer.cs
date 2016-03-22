using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnLogicTimer : MonoBehaviour
{

	private List<int> m_indices = new List<int>();
	private List<Vector3> m_vertices = new List<Vector3>();
	private List<Vector3> m_normals = new List<Vector3>();
	private List<Vector2> m_uvs = new List<Vector2>();

	private static readonly Vector3 sm_up = new Vector3(0, 0, -1);
	private MeshRenderer m_renderer;
	private Mesh m_mesh;
	private SyncManager m_syncManager;
	private CanvasRenderer m_canvasRenderer;

	public struct Vertex
	{
		public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
		{
			this.position = position;
			this.normal = normal;
			this.uv = uv;
		}
		public Vector3 position;
		public Vector3 normal;
		public Vector2 uv;
	}



	void Start()
	{
		m_renderer = GetComponent<MeshRenderer>();
		m_canvasRenderer = GetComponent<CanvasRenderer>();
		m_mesh = GetComponent<MeshFilter>().mesh;
		m_mesh.MarkDynamic();
	}

	// Update is called once per frame
	void Update()
	{
		if(m_syncManager == null)
		{
			m_syncManager = FindObjectOfType<SyncManager>();
			return;
		}
		m_mesh.Clear();
		m_vertices.Clear();
		m_uvs.Clear();
		m_normals.Clear();
		m_indices.Clear();

		Vertex[] tri = new Vertex[3];
		Vector3 pos = new Vector3();
		Vector3 prevpos = new Vector3();
		tri[0].normal = tri[1].normal = tri[2].normal = -sm_up;

		float turnprogress = .0f;
		bool turnInProgress = m_syncManager.GetTurnProgress(out turnprogress);
		if (turnInProgress)
			turnprogress = 1;
		else
			turnInProgress = true;
		float angle = 2 * 3.1415f * turnprogress;
		

		for (float a = .0f; a < angle; a += .04f)
		{
			float ang = -3.1415f * .5f - a;
			pos = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0);
			tri[0].position = pos * 30;
			tri[1].position = prevpos * 30;
			tri[2].position = new Vector3();
			tri[0].uv = new Vector2(pos.x, pos.y) * .5f + new Vector2(.5f, .5f);
			tri[1].uv = new Vector2(prevpos.x, prevpos.y) * .5f + new Vector2(.5f, .5f);
			tri[2].uv = new Vector2(.5f, .5f);

			WriteTriangle(tri);
			prevpos = pos;
		}

		m_mesh.vertices = m_vertices.ToArray();
		m_mesh.normals = m_normals.ToArray();
		m_mesh.uv = m_uvs.ToArray();
		m_mesh.triangles = m_indices.ToArray();
		m_canvasRenderer.SetMesh(m_mesh);
	}

	private void WriteTriangle(Vertex[] v)
	{
		WriteVertex(v[0]);
		WriteVertex(v[1]);
		WriteVertex(v[2]);
	}
	private void WriteVertex(Vertex v)
	{
		m_vertices.Add(v.position);
		m_normals.Add(v.normal);
		m_uvs.Add(v.uv);
		m_indices.Add(m_vertices.Count - 1);
	}
}
