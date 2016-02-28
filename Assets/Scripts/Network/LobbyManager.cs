using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

[RequireComponent(typeof(CustomNetworkDiscovery))]
[RequireComponent(typeof(CustomNetworkLobbyManager))]
public class LobbyManager : Singleton<LobbyManager>
{
	public bool m_dedicatedServer = false;

	private CustomNetworkDiscovery m_discovery;
	private CustomNetworkLobbyManager m_manager;
	private System.Action m_onErrorCallback;
	private System.Action m_exitAction;

	// PHZ dedicated server, hardcoded for now...
	public const string dedicatedServerNetworkAddress = "84.248.74.249";
	public const string dedicatedServerLocalAddress = "0.0.0.0";
	public const int dedicatedServerNetworkPort = 20000;
	public const int dedicatedServerMaxPlayers = 8; // TODO max players support
	
	// TODO check when connecting
	//private int gameVersion = 1;

	public class BroadcastData
	{
		// TODO players and max players
		public int networkPort;
		public string gameName;
		public bool isValid;

		public BroadcastData()
		{
			networkPort = 0;
			gameName = "";
			isValid = false;
		}
	}

	public static string EncodeBroadcastData(BroadcastData data)
	{
		return data.networkPort + "|" + data.gameName;
	}

	public static BroadcastData DecodeBroadcastData(string stringData)
	{
		BroadcastData data = new BroadcastData();

		string[] splitData = stringData.Split('|');
		if (splitData.Length < 2)
		{
			Debug.LogError("Unable to decode broadcast data: '" + stringData + "'");
			return data;
		}

		data.networkPort = int.Parse(splitData[0]);
		data.gameName = splitData[1];
		data.isValid = true;
		return data;
	}

	void Awake()
	{
		m_discovery = GetComponent<CustomNetworkDiscovery>();
		m_manager = GetComponent<CustomNetworkLobbyManager>();
	}

	void Start()
	{
		

		if (m_dedicatedServer)
		{
			if (!StartDedicatedServerLobby())
			{
				Debug.LogError("Unable to start dedicated server!");
				Application.Quit();
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#endif
			}
		}
	}

	public void SetOnErrorCallback(System.Action onErrorCallback)
	{
		m_onErrorCallback = onErrorCallback;
		m_manager.SetOnErrorCallback(onErrorCallback);
	}

	public void SetOnConnectedCallback(System.Action onConnectedCallback)
	{
		m_manager.SetOnConnectedCallback(onConnectedCallback);
	}

	public void SetOnReceiveBroadcastCallback(System.Action<string, string> onReceiveBroadcast)
	{
		m_discovery.SetOnReceiveBroadcastCallback(onReceiveBroadcast);
	}

	public bool StartDedicatedServerLobby()
	{
		Debug.Log("Starting dedicated server lobby");

		m_manager.networkAddress = dedicatedServerLocalAddress;
		m_manager.networkPort = dedicatedServerNetworkPort;
		m_manager.maxPlayers = dedicatedServerMaxPlayers;
		
		return m_manager.StartServer();
	}

	public void StartHostLobby()
	{
		if (NetworkClient.active || NetworkServer.active)
		{
			m_manager.StopHost();
		}

		m_manager.networkAddress = GlobalSettings.hostNetworkAddress;
		m_manager.networkPort = GlobalSettings.hostNetworkPort;
		m_manager.maxPlayers = GlobalSettings.hostPlayerCount;

		Debug.Log("Host game lobby - IP: " + m_manager.networkAddress + " Port: " + m_manager.networkPort);

		NetworkClient client = m_manager.StartHost();

		if (client == null && m_onErrorCallback != null)
		{
			m_onErrorCallback();
		}

		m_exitAction = StopHostLobby;
	}

	public void StartHostBroadcasting()
	{
		BroadcastData data = new BroadcastData();
		data.networkPort = GlobalSettings.hostNetworkPort;
		data.gameName = GlobalSettings.hostGameName;
		m_discovery.broadcastData = EncodeBroadcastData(data);

		m_discovery.Initialize();
		m_discovery.StartAsServer();
	}

	public void StopHostBroadcasting()
	{
		if (m_discovery.running)
		{
			m_discovery.StopBroadcast();
		}
	}

	public void StopHostLobby()
	{
		Debug.Log("Stop lobby hosting");

		m_manager.StopHost();

		if (m_discovery.running)
		{
			m_discovery.StopBroadcast();
		}
	}

	public void StartListeningLocalBroadcasts()
	{
		m_discovery.Initialize();
		StartCoroutine(StartDiscoveryAsClient());
	}

	// For some magical reason we have to wait a while before starting
	// because otherwise after joining a game and returning to main menu
	// restarting discovery just fails...
	private IEnumerator StartDiscoveryAsClient()
	{
		yield return new WaitForSeconds(0.1f);
		m_discovery.StartAsClient();
	}

	public void StopListeningLocalBroadcasts()
	{
		if (m_discovery.running)
		{
			m_discovery.StopBroadcast();
		}
	}

	public void JoinLobbyAsClient(string networkAddress, int networkPort)
	{
		m_manager.networkAddress = networkAddress;
		m_manager.networkPort = networkPort;

		Debug.Log("Join game lobby - IP: " + m_manager.networkAddress + " Port: " + m_manager.networkPort);

		NetworkClient client = m_manager.StartClient();

		if (client == null && m_onErrorCallback != null)
		{
			m_onErrorCallback();
		}

		m_exitAction = ExitLobbyAsClient;
	}

	public void ExitLobbyAsClient()
	{
		Debug.Log("Exit lobby");

		if (m_discovery.running)
		{
			m_discovery.StopBroadcast();
		}

		m_manager.StopClient();
	}

	public void AddLobbyPlayer(CustomNetworkLobbyPlayer lobbyPlayer)
	{
		LobbyMenuScreen lobbyMenu = FindObjectOfType<LobbyMenuScreen>();
		lobbyMenu.AddLobbyPlayer(lobbyPlayer);
	}

	public void SetReady(bool ready)
	{
		// TODO?
	}

	public void ExitGame()
	{
		if (m_exitAction != null)
		{
			m_exitAction();
		}
		else
		{
			Application.Quit();
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
	}

}
