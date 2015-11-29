using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkManager))]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class JoinMenuScreen : NetworkBehaviour
{
	public GameObject m_serverPanel;
	public GameObject m_mainMenuPanel;
	public GameObject m_networkManager;

	private NetworkManager m_manager;

	void Awake()
	{
		m_manager = m_networkManager.GetComponent<NetworkManager>();
	}

	public void JoinGame()
	{
		if (!NetworkClient.active && !NetworkServer.active && m_manager.matchMaker == null)
		{
			m_manager.StartClient();
			m_serverPanel.SetActive(false);
		}
	}

	public void RefreshGameList()
	{
		//TODO: Implement refreshing
	}

	public void CloseJoinMenu()
	{
		m_serverPanel.SetActive(false);
		m_mainMenuPanel.SetActive(true);
	}
}
