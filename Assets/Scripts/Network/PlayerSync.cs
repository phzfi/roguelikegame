using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerSync : NetworkBehaviour
{
	[SerializeField]
	SimpleCharacterMovement m_mover;

	[SyncVar]
	Vector2i m_syncPosition;

	void Start()
	{
		if (isLocalPlayer) // change object color to green if this is the player's own cube
		{
			var renderer = GetComponent<MeshRenderer>();
			renderer.material.color = new Color(0, 1, 0);

			var mainCameraController = FindObjectOfType<MainCameraController>();
			mainCameraController.SetTarget(transform, true);
		}
		gameObject.name = (isLocalPlayer ? "Local" : "Remote") + "NetworkPlayer";

		m_syncPosition = MapGrid.WorldToGridPoint(transform.position);

	}

	void Update()
	{
		m_mover.m_gridPos = m_syncPosition;
	}

	void OnDestroy()
	{
	}

	public void SyncPosition(Vector2i pos)
	{
		m_syncPosition = pos;
	}

	public bool IsLocalPlayer()
	{
		return isLocalPlayer;
	}
}
