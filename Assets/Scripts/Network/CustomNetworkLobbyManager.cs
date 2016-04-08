using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class CustomNetworkLobbyManager : NetworkLobbyManager
{
	private SyncManager m_syncManager;
	private PlayerSpawner m_spawner;
	private NetworkReset m_resetter;

	[SerializeField]
	private MainMenuScreen m_mainMenu;

	private System.Action m_onErrorCallback;
	private System.Action m_onConnectedCallback;

	public void Awake()
	{
		m_spawner = GetComponent<PlayerSpawner>();
		m_resetter = FindObjectOfType<NetworkReset>();
	}

	void OnDestroy()
	{
		//StopServer();
		Network.Disconnect();
		Debug.Log("Network destroyed");
	}

	private IEnumerator ShowMainMenu()
	{
		m_mainMenu.Show();
		yield return null;
		m_mainMenu.Show();
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		float timestamp = Time.realtimeSinceStartup;
		Debug.Log("Incoming connection");
		base.OnServerConnect(conn);
		if (conn.connectionId >= maxConnections)
		{
			conn.Disconnect();
			conn.Dispose();
		}
	}
	
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
				StartCoroutine(ShowMainMenu());

				StopServer();
				//Debug.Log("Restarting dedicated server lobby");
				//StartCoroutine(RestartServerRoutine());
			}
		}
	}

	private IEnumerator RestartServerRoutine()
	{
		yield return new WaitForSeconds(1.0f);
		StopServer();
		yield return new WaitForSeconds(1.0f);
		Debug.Log("Starting server");
		StartServer();
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
        gamePlayer.GetComponent<CharController>().m_name = lobbyPlayer.GetComponent<CustomNetworkLobbyPlayer>().m_playerName;
        return true;
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		base.OnServerSceneChanged(sceneName);

		if(Application.loadedLevelName == playScene)
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
		StartCoroutine(ShowMainMenu());
		Debug.Log("OnStopServer");
		base.OnStopServer();
		if (m_syncManager)
			m_syncManager.StopOnServer();
		m_resetter.RestartNetwork();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		Debug.Log("OnStartServer (lobby)");
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
		SyncManager.IncrementClientCount();
		if (m_spawner == null)
			m_spawner = GetComponent<PlayerSpawner>();
		return m_spawner.Spawn(conn, playerControllerId);
		//return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
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
			m_onErrorCallback = null;
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
		StartCoroutine(ShowMainMenu());
		//StartCoroutine(ShowMainMenu());

		if (m_onErrorCallback != null)
		{
			m_onErrorCallback();
		}

		if(!m_resetter.m_running)
			m_resetter.RestartNetwork();
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		Debug.LogError("ClientError: " + errorCode);
		base.OnClientError(conn, errorCode);
		
		if (m_onErrorCallback != null)
		{
			m_onErrorCallback();
		}
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		Debug.Log("OnClientDisconnect");
		base.OnClientDisconnect(conn);

		if (m_onErrorCallback != null)
		{
			m_onErrorCallback();
		}
	}
	
}
