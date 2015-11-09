using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkManager))]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class HostMenuScreen : NetworkBehaviour
{
    public GameObject m_mainMenuPanel;
    public GameObject m_hostGamePanel;
    public InputField m_serverName;
    public InputField m_IPaddress;
    public Dropdown m_playercount;
    public Toggle m_rankedMatch;
    public GameObject networkManager;

    private NetworkManager m_manager;

    void Awake()
    {
        m_manager = networkManager.GetComponent<NetworkManager>();
    }

    public void CloseHostMenu()
    {
        m_mainMenuPanel.SetActive(true);
        m_hostGamePanel.SetActive(false);
    }

    public void HostGame()
    {
        if (!NetworkClient.active && !NetworkServer.active && m_manager.matchMaker == null)
        {
            string serverName = m_serverName.text;
            m_manager.networkAddress = m_IPaddress.text;
            int playercount;
            bool ranked = m_rankedMatch.isOn;
            switch (m_playercount.value)
            {
                case 0:
                    playercount = 4;
                    break;
                case 1:
                    playercount = 6;
                    break;
                case 2:
                    playercount = 8;
                    break;
            };
            m_manager.StartHost();
            m_hostGamePanel.SetActive(false);
        }



        //TODO: Implement game hosting
    }


}
