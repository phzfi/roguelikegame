using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TileRenderer : MonoBehaviour {

	public string m_name;
	public enum TileRendererType { FogOfWarOpen = 0, Movement, FogOfWarClosed};
	public TileRendererType m_type;

	private List<int> m_indices = new List<int>();
	private List<Vector3> m_vertices = new List<Vector3>();
	private List<Vector3> m_normals = new List<Vector3>();
	private List<Vector2> m_uvs = new List<Vector2>();

	private static readonly Vector3 sm_up = new Vector3(0, 0, -1);
	private MeshRenderer m_meshRenderer;
	private Mesh m_mesh;

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
		m_meshRenderer = GetComponent<MeshRenderer>();
		m_mesh = GetComponent<MeshFilter>().mesh;
		m_mesh.name = m_name;
		m_mesh.MarkDynamic();
	}

	public void Show()
	{
		m_meshRenderer.enabled = true;
	}

	public void Hide()
	{
		m_meshRenderer.enabled = false;
	}

	public void CreateMesh(Vector3[] tileCoords)
	{
		m_mesh.Clear();
		m_vertices.Clear();
		m_uvs.Clear();
		m_normals.Clear();
		m_indices.Clear();

		Vertex[] quad = new Vertex[4];
		for (int i = 0; i < tileCoords.Length; ++i)
		{
			Vector3 currentPos = tileCoords[i];
			quad[0].normal = quad[1].normal = quad[2].normal = quad[3].normal = sm_up;

			quad[0].position = currentPos + new Vector3(1, 1, 0) * .5f;
			quad[0].uv = new Vector3(1, 1, 0);

			quad[1].position = currentPos + new Vector3(-1, 1, 0) * .5f;
			quad[1].uv = new Vector3(0, 1, 0);

			quad[2].position = currentPos + new Vector3(1, -1, 0) * .5f;
			quad[2].uv = new Vector3(1, 0, 0);

			quad[3].position = currentPos + new Vector3(-1, -1, 0) * .5f;
			quad[3].uv = new Vector3(0, 0, 0);

			WriteQuad(quad);
		}

		m_mesh.vertices = m_vertices.ToArray();
		m_mesh.normals = m_normals.ToArray();
		m_mesh.uv = m_uvs.ToArray();
		m_mesh.triangles = m_indices.ToArray();
	}

	private void WriteQuad(Vertex[] v)
	{
		WriteVertex(v[0]);
		WriteVertex(v[2]);
		WriteVertex(v[3]);

		WriteVertex(v[0]);
		WriteVertex(v[3]);
		WriteVertex(v[1]);
	}

	private void WriteVertex(Vertex v)
	{
		m_vertices.Add(v.position);
		m_normals.Add(v.normal);
		m_uvs.Add(v.uv);
		m_indices.Add(m_vertices.Count - 1);
	}

}
