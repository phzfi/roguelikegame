﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour
{
	public GameObject m_prefab;

	public void Start()
	{
		GameObject obj = (GameObject)Instantiate(m_prefab, transform.position, transform.rotation);
		NetworkServer.Spawn(obj);
		Destroy(gameObject);
	}
}
