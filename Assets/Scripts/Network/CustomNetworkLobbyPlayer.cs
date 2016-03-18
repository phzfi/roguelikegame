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

	[SyncVar(hook = "OnReadyStateChanged")]
	public bool m_readyToStart = false;

	public void ReadyButtonPressed()
	{
		m_readyButton.gameObject.SetActive(false);
		m_cancelButton.gameObject.SetActive(true);

		SendReadyToBeginMessage();
		CmdReadyStateChanged(true);
	}

	public void CancelButtonPressed()
	{
		m_cancelButton.gameObject.SetActive(false);
		m_readyButton.gameObject.SetActive(true);

		SendNotReadyToBeginMessage();
		CmdReadyStateChanged(false);
	}

	public override void OnClientReady(bool readyState)
	{
		base.OnClientReady(readyState);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
	}

	public override void OnClientEnterLobby()
	{
		base.OnClientEnterLobby();

		OnPlayerNameChanged(m_playerName);
		OnReadyStateChanged(m_readyToStart);

		LobbyManager.Instance.AddLobbyPlayer(this);
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		CmdNameChanged(GlobalSettings.playerName);
		CmdReadyStateChanged(false);

		m_readyButton.gameObject.SetActive(true);
	}

	public void OnPlayerNameChanged(string newName)
	{
		m_playerName = newName;
		m_nameText.text = m_playerName;
	}

	public void OnReadyStateChanged(bool ready)
	{
		m_readyToStart = ready;

		m_readyText.gameObject.SetActive(ready);
		m_notReadyText.gameObject.SetActive(!ready);
	}

	[Command]
	public void CmdNameChanged(string name)
	{
		m_playerName = name;
	}

	[Command]
	public void CmdReadyStateChanged(bool ready)
	{
		m_readyToStart = ready;
	}

}
