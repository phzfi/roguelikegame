using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class CustomNetworkLobbyManager : NetworkLobbyManager
{
	private SyncManager m_syncManager;

	[SerializeField]
	private MainMenuScreen m_mainMenu;

	private System.Action m_onErrorCallback;
	private System.Action m_onConnectedCallback;

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
		Debug.Log("Client disconnected");
		if (m_syncManager)
		{
			m_syncManager.DisconnectClient(conn);

			if (m_syncManager.GetClientCount() == 0)
			{
				Debug.Log("All clients disconnected - returning to main menu");
				m_syncManager.StopOnServer();
				m_mainMenu.Show();
			}
		}
	}

	// Lobby Manager
	public void SetOnErrorCallback(System.Action onErrorCallback)
	{
		m_onErrorCallback = onErrorCallback;
	}

	public void SetOnConnectedCallback(System.Action onConnectedCallback)
	{
		m_onConnectedCallback = onConnectedCallback;
	}

	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
	{
		// TODO configure game player based on lobby player
		return true;
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		base.OnServerSceneChanged(sceneName);

		if (Application.loadedLevelName == playScene)
		{
			Debug.Log("Start game on server!");
			m_syncManager = FindObjectOfType<SyncManager>();
			if (m_syncManager == null)
				Debug.LogError("Unable to find SyncManager");
			m_mainMenu.HideAll();
		}
	}

	public override void OnStopServer()
	{
		Debug.Log("OnStopServer");
		base.OnStopServer();
		if (m_syncManager)
			m_syncManager.StopOnServer();
		m_mainMenu.Show();
	}

	// Server / Host
	public override void OnLobbyStartServer()
	{
		base.OnLobbyStartServer();
	}

	public override void OnLobbyStartHost()
	{
		base.OnLobbyStartHost();
	}

	public override void OnLobbyStopHost()
	{
		base.OnLobbyStopHost();
	}

	public override void OnLobbyServerPlayersReady()
	{
		base.OnLobbyServerPlayersReady();
	}

	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
	{
		return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
	}

	public override void OnServerError(NetworkConnection conn, int errorCode)
	{
		base.OnServerError(conn, errorCode);
		Debug.LogError("Server Error: " + errorCode);

		if (m_onErrorCallback != null)
		{
			m_onErrorCallback();
		}
	}

	// Client
	public override void OnLobbyClientEnter()
	{
		base.OnLobbyClientEnter();

		if (m_onConnectedCallback != null)
		{
			m_onConnectedCallback();
		}
	}

	public override void OnLobbyClientExit()
	{
		base.OnLobbyClientExit();

		if (m_onErrorCallback != null)
		{
			m_onErrorCallback();
		}
	}

	public override void OnLobbyStopClient()
	{
		base.OnLobbyStopClient();
	}

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		if (Application.loadedLevelName == playScene)
		{
			Debug.Log("Start game on client!");
			// TODO figure out a more robust way to get the sync manager
			m_syncManager = GameObject.Find("Managers").transform.GetComponentsInChildren<SyncManager>(true)[0];
			if (m_syncManager == null)
				Debug.LogError("Unable to find SyncManager");
			m_mainMenu.HideAll();
		}

		base.OnClientSceneChanged(conn);
	}

	public override void OnStopClient()
	{
		Debug.Log("OnStopClient");
		base.OnStopClient();
		if (m_syncManager)
			m_syncManager.StopOnClient();
		m_mainMenu.Show();
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		base.OnClientError(conn, errorCode);
		Debug.LogError("ClientError: " + errorCode);

		if (m_onErrorCallback != null)
		{
			m_onErrorCallback();
		}
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		Debug.Log("OnClientDisconnect");
		base.OnClientDisconnect(conn);
	}
	
}
