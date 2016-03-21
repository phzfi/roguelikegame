using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class JoinMenuScreen : MenuScreen
{
	public MainMenuScreen m_mainMenu;
	public LobbyMenuScreen m_lobbyMenu;
	public ErrorMessage m_errorMessage;
	public WaitMessage m_connectingMessage;
	public Button m_joinButton;
	public Transform m_remoteServerLabelsParent;
	public Transform m_localServerLabelsParent;
	public ServerLabel m_directConnectServerLabel;
	public ToggleGroup m_toggleGroup;

	private bool m_connecting = false;
	private int m_selectedServerIndex = -1;
	private ServerLabel[] m_remoteServerLabels = new ServerLabel[0];
	private ServerLabel[] m_localServerLabels = new ServerLabel[0];

	private const float m_disconnectTime = 2.0f;

	private enum ServerType
	{
		Dedicated,
		LocalHost,
		DirectConnect
	}

	private class ServerInfo
	{
		public ServerInfo()
		{
			serverType = ServerType.Dedicated;
			networkAddress = "";
			networkPort = 0;
			gameName = "";
			players = 0;
			maxPlayers = 0;
			lastSeenTime = Time.time;
			label = null;
		}

		public ServerType serverType;
		public string networkAddress;
		public int networkPort;
		public string gameName;
		public int players;
		public int maxPlayers;
		public float lastSeenTime;
		public ServerLabel label;
	}

	private List<ServerInfo> m_serverInfos = new List<ServerInfo>();
	private ServerInfo m_joinedServerInfo = null;

	protected override void Start()
	{
		base.Start();

		m_remoteServerLabels = m_remoteServerLabelsParent.GetComponentsInChildren<ServerLabel>(true);
		m_localServerLabels = m_localServerLabelsParent.GetComponentsInChildren<ServerLabel>(true);
	}

	public override void Show()
	{
		base.Show();

		m_selectedServerIndex = -1;
		m_serverInfos.Clear();
		m_toggleGroup.SetAllTogglesOff();

		ServerInfo directConnect = new ServerInfo();
		directConnect.serverType = ServerType.DirectConnect;
		directConnect.networkAddress = GlobalSettings.joinNetworkAddress;
		directConnect.networkPort = GlobalSettings.joinNetworkPort;
		directConnect.gameName = "Direct Connect";
		m_serverInfos.Add(directConnect);
		m_directConnectServerLabel.SetIndex(0);
		m_directConnectServerLabel.UpdateTexts("", directConnect.networkAddress, directConnect.networkPort, 0, 0);
		m_directConnectServerLabel.Show();

		ServerInfo phzServer = new ServerInfo();
		phzServer.serverType = ServerType.Dedicated;
		phzServer.networkAddress = LobbyManager.dedicatedServerNetworkAddress;
		phzServer.networkPort = LobbyManager.dedicatedServerNetworkPort;
		phzServer.gameName = "Official PHZ Server";
		m_serverInfos.Add(phzServer);

		for (int i = 0; i < m_remoteServerLabels.Length; ++i)
			m_remoteServerLabels[i].Hide();

		for (int i = 0; i < m_localServerLabels.Length; ++i)
			m_localServerLabels[i].Hide();

		RefreshServersList();

		LobbyManager.Instance.StartListeningLocalBroadcasts();
		LobbyManager.Instance.SetOnReceiveBroadcastCallback(OnReceiveBroadcast);
	}

	public override void Hide()
	{
		m_selectedServerIndex = -1;
		m_serverInfos.Clear();
		m_toggleGroup.SetAllTogglesOff();

		LobbyManager.Instance.StopListeningLocalBroadcasts();
		LobbyManager.Instance.SetOnReceiveBroadcastCallback(null);

		GlobalSettings.SaveToDisk();

		base.Hide();
	}

	public void Update()
	{
		m_joinButton.interactable = m_selectedServerIndex >= 0 && !m_connecting;

		bool dirty = false;

		for (int i = 0; i < m_serverInfos.Count; ++i)
		{
			if (m_serverInfos[i].serverType != ServerType.LocalHost)
				continue;

			float timeSinceLastUpdate = Time.time - m_serverInfos[i].lastSeenTime;
			if (timeSinceLastUpdate >= m_disconnectTime)
			{
				if (m_selectedServerIndex == i)
					m_selectedServerIndex = -1;

				if (m_serverInfos[i].label)
					m_serverInfos[i].label.Hide();

				m_serverInfos.RemoveAt(i);
				dirty = true;
				i--;
			}
		}

		if (dirty)
		{
			RefreshServersList();
		}
	}

	public void JoinButtonPressed()
	{
		if (m_connecting)
			return;

		m_connecting = true;
		SetButtonsEnabled(false);
		m_connectingMessage.Show("Joining a game...", OnConnectingCanceled);

		m_joinedServerInfo = m_serverInfos[m_selectedServerIndex];
		
		LobbyManager.Instance.SetOnErrorCallback(OnJoiningFailed);
		LobbyManager.Instance.SetOnConnectedCallback(OnJoiningSuccesfull);
		LobbyManager.Instance.JoinLobbyAsClient(m_joinedServerInfo.networkAddress, m_joinedServerInfo.networkPort);
	}

	public void BackButtonPressed()
	{
		if (m_connecting)
			OnConnectingCanceled();

		Hide();
		m_mainMenu.Show();
	}

	public void OnJoiningSuccesfull()
	{
		m_connecting = false;
		m_connectingMessage.Hide();
		SetButtonsEnabled(true);
		//LobbyManager.Instance.SetOnErrorCallback(null);
		LobbyManager.Instance.SetOnConnectedCallback(null);

		Hide();
		m_lobbyMenu.SetIsHost(false);
		m_lobbyMenu.SetGameName(m_joinedServerInfo.gameName);
		m_lobbyMenu.Show();
	}

	public void OnJoiningFailed()
	{
		m_connecting = false;
		m_connectingMessage.Hide();
		SetButtonsEnabled(true);
		m_errorMessage.Show("Failed to join a game!");

		LobbyManager.Instance.SetOnErrorCallback(null);
		LobbyManager.Instance.SetOnConnectedCallback(null);
		LobbyManager.Instance.ExitLobbyAsClient();

		Show();
		m_lobbyMenu.Hide();
	}

	public void OnConnectingCanceled()
	{
		m_connecting = false;
		SetButtonsEnabled(true);

		LobbyManager.Instance.SetOnErrorCallback(null);
		LobbyManager.Instance.SetOnConnectedCallback(null);
		LobbyManager.Instance.ExitLobbyAsClient();
	}

	public void SelectServer(int index)
	{
		if (index >= m_serverInfos.Count)
		{
			Debug.LogError(string.Format("Server index out of bounds: {0}/{1}", index, m_serverInfos.Count));
			m_selectedServerIndex = -1;
		}
		else
		{
			m_selectedServerIndex = index;
		}
	}

	public void DirectIPAddressChanged(string address)
	{
		GlobalSettings.joinNetworkAddress.Set(address);
		int directConnectLabelIndex = m_directConnectServerLabel.GetIndex();
		m_serverInfos[directConnectLabelIndex].networkAddress = address;
	}

	public void DirectIPPortChanged(string port)
	{
		int parsedPort = 0;
		if (int.TryParse(port, out parsedPort))
		{
			GlobalSettings.joinNetworkPort.Set(parsedPort);
			int directConnectLabelIndex = m_directConnectServerLabel.GetIndex();
			m_serverInfos[directConnectLabelIndex].networkPort = parsedPort;
		}
	}

	public void OnReceiveBroadcast(string fromAddress, string stringData)
	{
		LobbyManager.BroadcastData data = LobbyManager.DecodeBroadcastData(stringData);

		if (data.isValid)
		{
			// For some reason fromAddress is prefixed with "::ffff:"
			if (fromAddress.StartsWith("::ffff:"))
			{
				fromAddress = fromAddress.Substring(7);
			}

			// ... And there might be junk like "%6" at the end
			fromAddress = fromAddress.Split('%')[0];

			ServerInfo info = new ServerInfo();
			info.serverType = ServerType.LocalHost;
			info.networkAddress = fromAddress;
			info.networkPort = data.networkPort;
			info.gameName = data.gameName;
			// TODO players and max players

			AddLocalHostServer(info);
		}
	}

	private void AddLocalHostServer(ServerInfo info)
	{
		for (int i = 0; i < m_serverInfos.Count; ++i)
		{
			if (m_serverInfos[i].networkAddress == info.networkAddress)
			{
				ServerInfo updatedInfo = m_serverInfos[i];
				updatedInfo.networkPort = info.networkPort;
				updatedInfo.gameName = info.gameName;
				updatedInfo.lastSeenTime = info.lastSeenTime;
				m_serverInfos[i] = updatedInfo;

				bool dirty = false;
				dirty |= (m_serverInfos[i].networkPort != info.networkPort);
				dirty |= (m_serverInfos[i].gameName != info.gameName);
				dirty |= (m_serverInfos[i].players != info.players);
				dirty |= (m_serverInfos[i].maxPlayers != info.maxPlayers);

				if (dirty)
				{
					RefreshServersList();
				}
				return;
			}
		}

		m_serverInfos.Add(info);
		RefreshServersList();
	}

	private void RefreshServersList()
	{
		for (int i = 0; i < m_serverInfos.Count; ++i)
		{
			ServerInfo info = m_serverInfos[i];
			ServerLabel label = info.label;

			if (label == null)
			{
				if (info.serverType == ServerType.Dedicated)
				{
					label = GetUnusedLabel(ref m_remoteServerLabels);
				}
				else if (info.serverType == ServerType.LocalHost)
				{
					label = GetUnusedLabel(ref m_localServerLabels);
				}
			}

			if (label != null)
			{
				label.SetIndex(i);
				label.UpdateTexts(info.gameName, info.networkAddress, info.networkPort, info.players, info.maxPlayers);
				label.Show();
				m_serverInfos[i].label = label;
			}
		}
	}

	private ServerLabel GetUnusedLabel(ref ServerLabel[] labels)
	{
		for (int i = 0; i < labels.Length; ++i)
		{
			if (!labels[i].IsVisible())
			{
				return labels[i];
			}
		}

		Debug.LogError("Join menu screen ran out of server labels!");
		return null;
	}
}
