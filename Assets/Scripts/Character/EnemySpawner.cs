using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour
{
	public GameObject m_prefab;

	public void Start()
	{
		GameObject obj = (GameObject)Instantiate(m_prefab, transform.position, m_prefab.transform.rotation);
		var controller = obj.GetComponent<CharController>();
		controller.ID = CharManager.GetNextID();
		NetworkServer.Spawn(obj);
		Destroy(gameObject);
	}
}
