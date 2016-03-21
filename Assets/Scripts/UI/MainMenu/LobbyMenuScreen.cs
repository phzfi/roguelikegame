using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LobbyMenuScreen : MenuScreen
{
	public HostMenuScreen m_hostMenu;
	public JoinMenuScreen m_joinMenu;
	public ErrorMessage m_errorMessage;
	public Transform m_playerListParent;
	public Text m_gameNameLabel;
	public Button m_readyButton;

	private bool m_isReady = false;
	private bool m_isHost = false;
	private const float m_playerLabelOffset = -35.0f;

	public void SetIsHost(bool isHost)
	{
		m_isHost = isHost;
	}

	public void SetGameName(string gameName)
	{
		m_gameNameLabel.text = gameName;
	}

	public override void Show()
	{
		base.Show();

		if (m_isHost)
		{
			LobbyManager.Instance.StartHostBroadcasting();
		}
	}

	public override void Hide()
	{
		base.Hide();

		if (m_isHost)
		{
			LobbyManager.Instance.StopHostBroadcasting();
		}
	}

	public void ReadyButtonPressed()
	{
		m_isReady = !m_isReady;
		LobbyManager.Instance.SetReady(m_isReady);
	}

	public void BackButtonPressed()
	{
		if (m_isHost)
		{
			LobbyManager.Instance.StopHostLobby();
		}
		else
		{
			LobbyManager.Instance.ExitLobbyAsClient();
		}

		Hide();

		if (m_isHost)
		{
			m_hostMenu.Show();
		}
		else
		{
			m_joinMenu.Show();
		}
	}

	public void AddLobbyPlayer(CustomNetworkLobbyPlayer lobbyPlayer)
	{
		lobbyPlayer.transform.SetParent(m_playerListParent, false);

		Vector3 localPos = Vector3.zero;
		localPos.y = lobbyPlayer.slot * m_playerLabelOffset;
		lobbyPlayer.transform.localPosition = localPos;
	}

}
