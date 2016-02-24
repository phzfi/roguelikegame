using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class CustomNetworkLobbyManager : NetworkLobbyManager
{
	[SerializeField]
	private SyncManager m_syncManager;

	[SerializeField]
	private MainMenuScreen m_mainMenu;

	private System.Action m_onErrorCallback;
	private System.Action m_onConnectedCallback;
	
	public void Start()
	{
		m_syncManager.enabled = false;
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
		m_syncManager.DisconnectClient(conn);
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
			m_syncManager.InitOnServer();
			m_mainMenu.HideAll();
		}
		else if (Application.loadedLevelName == lobbyScene)
		{
			Debug.Log("Start lobby on server!");
			m_syncManager.StopOnServer();
			m_mainMenu.Show();
		}
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
			m_syncManager.InitOnClient(conn);
			m_mainMenu.HideAll();
		}
		else if (Application.loadedLevelName == lobbyScene)
		{
			Debug.Log("Start lobby on client!");
			m_syncManager.StopOnClient();
			m_mainMenu.Show();
		}

		base.OnClientSceneChanged(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
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

}
