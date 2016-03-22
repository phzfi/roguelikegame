using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TileRenderer : MonoBehaviour {

	public string m_name;
	public enum TileRendererType { FogOfWarOpen = 0, Movement, FogOfWarClosed, MinimapWalls, MinimapItems, MinimapPlayers, Range };
	public TileRendererType m_type;
	public float m_tileSize = .5f;

	private List<int> m_indices = new List<int>();
	private List<Vector3> m_vertices = new List<Vector3>();
	private List<Vector3> m_normals = new List<Vector3>();
	private List<Vector2> m_uvs = new List<Vector2>();

	private static readonly Vector3 sm_up = new Vector3(0, 0, -1);
	private MeshRenderer m_meshRenderer;
	private Mesh m_mesh;

	private bool m_visible = false;
	private float m_alpha = 0.0f;
	private const float m_alphaFadeSpeed = 10.0f;

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
		if (m_visible)
			return;

		m_visible = true;
		StopAllCoroutines();
		StartCoroutine(ShowCoroutine());
	}

	private IEnumerator ShowCoroutine()
	{
		m_meshRenderer.enabled = true;
		while (m_alpha <= 1.0f)
		{
			SetAlpha(m_alpha);
			m_alpha += m_alphaFadeSpeed * Time.deltaTime;
			yield return null;
		}
		SetAlpha(1.0f);
	}

	public void Hide()
	{
		if (!m_visible)
			return;

		m_visible = false;
		StopAllCoroutines();
		StartCoroutine(HideCoroutine());
	}

	private IEnumerator HideCoroutine()
	{
		while (m_alpha >= 0.0f)
		{
			SetAlpha(m_alpha);
			m_alpha -= m_alphaFadeSpeed * Time.deltaTime;
			yield return null;
		}
		SetAlpha(0.0f);
		m_meshRenderer.enabled = false;
	}

	private void SetAlpha(float alpha)
	{
		Color c = m_meshRenderer.sharedMaterial.color;
		c.a = Mathf.Clamp01( alpha );
		m_meshRenderer.sharedMaterial.color = c;
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

			quad[0].position = currentPos + new Vector3(1, 1, 0) * m_tileSize;
			quad[0].uv = new Vector3(1, 1, 0);

			quad[1].position = currentPos + new Vector3(-1, 1, 0) * m_tileSize;
			quad[1].uv = new Vector3(0, 1, 0);

			quad[2].position = currentPos + new Vector3(1, -1, 0) * m_tileSize;
			quad[2].uv = new Vector3(1, 0, 0);

			quad[3].position = currentPos + new Vector3(-1, -1, 0) * m_tileSize;
			quad[3].uv = new Vector3(0, 0, 0);

			WriteQuad(quad);
		}

		m_mesh.vertices = m_vertices.ToArray();
		m_mesh.normals = m_normals.ToArray();
		m_mesh.uv = m_uvs.ToArray();
		m_mesh.triangles = m_indices.ToArray();
	}

	public void CreateMovementRangeMesh(HashSet<Vector2i> map)
	{
		m_mesh.Clear();
		m_vertices.Clear();
		m_uvs.Clear();
		m_normals.Clear();
		m_indices.Clear();

		Vertex[] quad = new Vertex[4];
		foreach ( var pos in map )
		{
			bool N = map.Contains(pos + Vector2i.UnitY);
			bool E = map.Contains(pos + Vector2i.UnitX);
			bool S = map.Contains(pos - Vector2i.UnitY);
			bool W = map.Contains(pos - Vector2i.UnitX);

			bool NE = map.Contains(pos + Vector2i.UnitX + Vector2i.UnitY);
			bool SE = map.Contains(pos + Vector2i.UnitX - Vector2i.UnitY);
			bool SW = map.Contains(pos - Vector2i.UnitX - Vector2i.UnitY);
			bool NW = map.Contains(pos - Vector2i.UnitX + Vector2i.UnitY);

			float uvX = 0.0f;
			if (!N) WriteMovementGridQuad(quad, pos, uvX, 0);
			if (!E) WriteMovementGridQuad(quad, pos, uvX, 1);
			if (!S) WriteMovementGridQuad(quad, pos, uvX, 2);
			if (!W) WriteMovementGridQuad(quad, pos, uvX, 3);
			
			uvX = 0.5f;
			if (N && E && !NE) WriteMovementGridQuad(quad, pos, uvX, 1);
			if (E && S && !SE) WriteMovementGridQuad(quad, pos, uvX, 2);
			if (S && W && !SW) WriteMovementGridQuad(quad, pos, uvX, 3);
			if (W && N && !NW) WriteMovementGridQuad(quad, pos, uvX, 0);

			/*
			int neighbourCount = (N ? 1 : 0) + (S ? 1 : 0) + (W ? 1 : 0) + (E ? 1 : 0);
			float uvX = 0.5f; // 0.0f;
			
			if (neighbourCount == 4)
				continue;

			int rotations = 0;
			if (neighbourCount == 1)
			{
				if (E) rotations = 1;
				if (S) rotations = 2;
				if (W) rotations = 3;
			}
			else if (neighbourCount == 2)
			{
				if (!E && !S) rotations = 1;
				if (!S && !W) rotations = 2;
				if (!W && !N) rotations = 3;
			}
			else if (neighbourCount == 3)
			{
				if (!E) rotations = 1;
				if (!S) rotations = 2;
				if (!W) rotations = 3;
			}
			*/


			//WriteMovementGridQuad(quad, pos, uvX, rotation);


		}
		
		m_mesh.vertices = m_vertices.ToArray();
		m_mesh.normals = m_normals.ToArray();
		m_mesh.uv = m_uvs.ToArray();
		m_mesh.triangles = m_indices.ToArray();
	}

	private void WriteMovementGridQuad( Vertex[] quad, Vector2i pos, float uvX, int rotation )
	{
		float halftileSize = 0.5f * MapGrid.tileSize;
		Vector3 currentPos = MapGrid.GridToWorldPoint(pos);
		quad[0].normal = quad[1].normal = quad[2].normal = quad[3].normal = sm_up;

		quad[0].position = currentPos + new Vector3(1, 1, 0) * halftileSize;
		quad[0].uv = new Vector3(uvX + 0.5f, 1, 0);

		quad[1].position = currentPos + new Vector3(-1, 1, 0) * halftileSize;
		quad[1].uv = new Vector3(uvX, 1, 0);

		quad[2].position = currentPos + new Vector3(1, -1, 0) * halftileSize;
		quad[2].uv = new Vector3(uvX + 0.5f, 0, 0);

		quad[3].position = currentPos + new Vector3(-1, -1, 0) * halftileSize;
		quad[3].uv = new Vector3(uvX, 0, 0);

		RotateUVs(ref quad, rotation);

		WriteQuad(quad);
	}

	private void RotateUVs(ref Vertex[] quad, int cwRotations)
	{
		for (int i = 0; i < cwRotations; ++i )
		{
			Vector2 tmp = quad[0].uv;
			quad[0].uv = quad[1].uv;
			quad[1].uv = quad[3].uv;
			quad[3].uv = quad[2].uv;
			quad[2].uv = tmp;
		}
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
