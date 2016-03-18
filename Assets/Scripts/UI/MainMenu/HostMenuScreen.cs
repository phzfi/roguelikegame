using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HostMenuScreen : MenuScreen
{
	public MainMenuScreen m_mainMenu;
	public LobbyMenuScreen m_lobbyMenu;
	public ErrorMessage m_errorMessage;
	public WaitMessage m_connectingMessage;
	public InputField m_serverName;
	public InputField m_IPaddress;
	public InputField m_port;
	public Dropdown m_playercount;

	private int[] m_playerCounts = { 2, 3, 4 };
	private bool m_connecting = false;

	protected override void Start()
	{
		base.Start();
		LoadSettings();
	}

	public override void Show()
	{
		LoadSettings();
		base.Show();
	}

	public override void Hide()
	{
		base.Hide();
		GlobalSettings.SaveToDisk();
	}

	public void HostButtonPressed()
	{
		if (m_connecting)
			return;

		m_connecting = true;
		SetButtonsEnabled(false);
		m_connectingMessage.Show("Hosting a game...", OnConnectingCanceled);

		LobbyManager.Instance.SetOnErrorCallback(OnHostingFailed);
		LobbyManager.Instance.SetOnConnectedCallback(OnHostingSuccessful);
		LobbyManager.Instance.StartHostLobby();
	}

	public void OnHostingSuccessful()
	{
		m_connecting = false;
		m_connectingMessage.Hide();
		SetButtonsEnabled(true);
		LobbyManager.Instance.SetOnErrorCallback(null);
		LobbyManager.Instance.SetOnConnectedCallback(null);

		Hide();
		m_lobbyMenu.SetIsHost(true);
		m_lobbyMenu.SetGameName(GlobalSettings.hostGameName);
		m_lobbyMenu.Show();
	}

	public void OnHostingFailed()
	{
		m_connecting = false;
		m_connectingMessage.Hide();
		SetButtonsEnabled(true);
		m_errorMessage.Show("Failed to host a game!");

		LobbyManager.Instance.SetOnErrorCallback(null);
		LobbyManager.Instance.SetOnConnectedCallback(null);
		LobbyManager.Instance.StopHostLobby();
	}

	public void OnConnectingCanceled()
	{
		m_connecting = false;
		SetButtonsEnabled(true);

		LobbyManager.Instance.SetOnErrorCallback(null);
		LobbyManager.Instance.SetOnConnectedCallback(null);
		LobbyManager.Instance.StopHostLobby();
	}

	public void BackButtonPressed()
	{
		if (m_connecting)
			OnConnectingCanceled();

		Hide();
		m_mainMenu.Show();
	}

	public void GameNameChanged(string name)
	{
		GlobalSettings.hostGameName.Set(name);
	}

	public void IPAddressChanged(string value)
	{
		GlobalSettings.hostNetworkAddress.Set(value);
	}

	public void PortChanged(string stringvalue)
	{
		int integerValue = 0;
		if (int.TryParse(stringvalue, out integerValue))
		{
			GlobalSettings.hostNetworkPort.Set(integerValue);
		}
		else
		{
			Debug.LogError("Failed to parse string '" + stringvalue + "' to integer!");
		}
	}

	public void PlayercountChanged(int value)
	{
		GlobalSettings.hostPlayerCount.Set(m_playerCounts[value]);
	}

	private void LoadSettings()
	{
		m_serverName.text = GlobalSettings.hostGameName;
		m_IPaddress.text = GlobalSettings.hostNetworkAddress;
		m_port.text = GlobalSettings.hostNetworkPort.Value.ToString();
		m_playercount.value = PlayerCountToDropDownIndex(GlobalSettings.hostPlayerCount);
		GlobalSettings.hostPlayerCount.Set(m_playerCounts[m_playercount.value]);
	}

	private int PlayerCountToDropDownIndex(int playerCount)
	{
		for (int i = 0; i < m_playerCounts.Length; ++i)
		{
			if (m_playerCounts[i] == playerCount)
			{
				return i;
			}
			else if (m_playerCounts[i] > playerCount)
			{
				return Mathf.Max(i - 1, 0);
			}
		}
		return 0;
	}

}
