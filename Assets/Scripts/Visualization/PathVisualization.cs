using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PathVisualization : MonoBehaviour
{
	public float m_lineWidth = 0.5f;
	public float m_lineTessellation = 0.5f;
	public float m_lineUVTiling = 1.0f;

	private Mesh m_mesh = null;
	private MeshRenderer m_meshRenderer = null;
	private List<Vector3> m_tessellatedPositions = new List<Vector3>();
	private List<int> m_indices = new List<int>();
	private List<Vector3> m_vertices = new List<Vector3>();
	private List<Vector3> m_normals = new List<Vector3>();
	private List<Vector2> m_uvs = new List<Vector2>();
	
	private static readonly Vector3 sm_up = new Vector3( 0, 0, -1 );

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
		m_mesh.name = "PathMesh";
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
	
	public void Create(List<Vector3> worldSpacePath)
	{
		m_tessellatedPositions.Clear();

		CatmullRomSpline spline = new CatmullRomSpline(worldSpacePath);

		for (int i = 0; i < spline.SectionCount; ++i)
		{
			float length = spline.GetSectionLength();
			int sectionCount = Mathf.CeilToInt(length / m_lineTessellation);
			float ooSectionCount = 1.0f / (float)sectionCount;

			for (int j = 0; j < sectionCount; j++)
			{
				m_tessellatedPositions.Add(spline.Interpolate(j * ooSectionCount, i));
			}
		}

		CreateMesh();
		Show();
	}
	
	private void CreateMesh()
	{
		m_mesh.Clear();
		m_indices.Clear();
		m_vertices.Clear();
		m_normals.Clear();
		m_uvs.Clear();
		
		float lineLength = 0.0f;
		int lastIndex = m_tessellatedPositions.Count;
        for (int i = 1; i < lastIndex; ++i)
		{
			Vector3 prevPos = m_tessellatedPositions[i - 1];
            Vector3 pos = m_tessellatedPositions[i];
			Vector3 nextPos = (i < lastIndex - 1) ? m_tessellatedPositions[i + 1] : pos + (pos - prevPos);
			
			Vector3 tangent = (pos - prevPos).normalized;
			Vector3 nextTangent = (nextPos - pos).normalized;

			Vector3 normal = Vector3.Cross(tangent, sm_up);
			Vector3 nextNormal = Vector3.Cross(nextTangent, sm_up);

			float distance = m_lineUVTiling * Vector3.Distance(prevPos, pos);

			Vertex[] vertices = {
				new Vertex( prevPos + normal * m_lineWidth, sm_up, new Vector2(lineLength, 0) ),
				new Vertex( prevPos - normal * m_lineWidth, sm_up, new Vector2(lineLength, 1) ),
				new Vertex( pos + nextNormal * m_lineWidth, sm_up, new Vector2(lineLength + distance, 0) ),
				new Vertex( pos - nextNormal * m_lineWidth, sm_up, new Vector2(lineLength + distance, 1) ),
			};

			WriteQuad(vertices);

			lineLength += distance;
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
