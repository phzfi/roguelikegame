using UnityEngine;
using System.Collections;

public class MiniMapCamera : MonoBehaviour {

	public GameObject m_target;

	void Start () 
	{

	}
	
	void LateUpdate () 
	{
		if (m_target != null && this != null) 
		{
			transform.position = new Vector3(m_target.transform.position.x, m_target.transform.position.y, transform.position.z);
		}
	}
}
