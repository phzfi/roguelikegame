using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class TextureScroll : MonoBehaviour
{
	public string m_textureName = "_MainTex";
	public float m_ScrollSpeedU = 1.0f;
	public float m_scrollSpeedV = 0.0f;

	private Material m_material = null;
	private Vector2 m_startOffset;

	void Start()
	{
		m_material = GetComponent<MeshRenderer>().material;
		
		if (m_material)
		{
			Debug.Assert(m_material.HasProperty(m_textureName), "TextureScroll: Unable to find texture '" + m_textureName + "'");
			m_startOffset = m_material.GetTextureOffset(m_textureName);
		}
    }
	
	void Update()
	{
		if (m_material)
		{
			Vector2 offset = m_startOffset + Time.time * new Vector2(m_ScrollSpeedU, m_scrollSpeedV);
            m_material.SetTextureOffset(m_textureName, offset);
		}
    }
}
