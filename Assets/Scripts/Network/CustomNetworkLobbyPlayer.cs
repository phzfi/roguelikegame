using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;

public class CustomNetworkLobbyPlayer : NetworkLobbyPlayer
{
	public Text m_nameText;
	public Text m_readyText;
	public Text m_notReadyText;
	public Button m_readyButton;
	public Button m_cancelButton;

	[SyncVar(hook = "OnPlayerNameChanged")]
	public string m_playerName = "";

	public void ReadyButtonPressed()
	{
		m_readyButton.gameObject.SetActive(false);
		m_cancelButton.gameObject.SetActive(true);

		SendReadyToBeginMessage();
	}

	public void CancelButtonPressed()
	{
		m_cancelButton.gameObject.SetActive(false);
		m_readyButton.gameObject.SetActive(true);

		SendNotReadyToBeginMessage();
	}

	public override void OnClientReady(bool readyState)
	{
		base.OnClientReady(readyState);

		m_readyText.gameObject.SetActive(readyState);
		m_notReadyText.gameObject.SetActive(!readyState);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
	}

	public override void OnClientEnterLobby()
	{
		base.OnClientEnterLobby();

		OnPlayerNameChanged(m_playerName);
		LobbyManager.Instance.AddLobbyPlayer(this);
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		CmdNameChanged(GlobalSettings.playerName);

		m_readyButton.gameObject.SetActive(true);
	}

	public void OnPlayerNameChanged(string newName)
	{
		m_playerName = newName;
		m_nameText.text = m_playerName;
	}

	[Command]
	public void CmdNameChanged(string name)
	{
		m_playerName = name;
	}

}
