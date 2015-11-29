using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
	[SerializeField]
	private SyncManager m_syncManager;

	public void Start()
	{
		m_syncManager.enabled = false;
	}

	public override void OnClientConnect(NetworkConnection conn) // called when connected to a server
	{
		base.OnClientConnect(conn);
		m_syncManager.InitOnClient(conn);
	}

	public override void OnStartServer() // called when starting server or host
	{
		base.OnStartServer();
		m_syncManager.InitOnServer();
		m_syncManager.enabled = true;
	}

	public override void OnServerDisconnect(NetworkConnection conn) // called on server (or host) when client disconnects
	{
		base.OnServerDisconnect(conn);
		m_syncManager.DisconnectClient(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn) // called on client when disconnected from a server
	{
		base.OnClientDisconnect(conn);
		m_syncManager.StopOnClient();
	}

	public override void OnStopServer() // called on server when server (or host) is stopped
	{
		base.OnStopServer();
		m_syncManager.StopOnServer();
	}
}
