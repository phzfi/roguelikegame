﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerSpawner : MonoBehaviour {
	public GameObject m_prefab;
	public GameObject Spawn(NetworkConnection conn, short playerControllerId)
	{
		GameObject obj = (GameObject)Instantiate(m_prefab, transform.position, m_prefab.transform.rotation);
		var controller = obj.GetComponent<CharController>();
		controller.ID = CharManager.GetNextID();

		var positions = FindObjectsOfType<NetworkStartPosition>();
		obj.transform.position = positions[controller.ID % 4].transform.position;
		NetworkServer.AddPlayerForConnection(conn, obj, playerControllerId);

		return obj;
	}
}
