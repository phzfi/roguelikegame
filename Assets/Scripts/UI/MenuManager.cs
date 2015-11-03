using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public GameObject m_serverPanel;
    public GameObject m_mainMenuPanel;
    public GameObject m_hostGamePanel;
    public InputField m_serverName;
    public InputField m_otherInput;
    public Dropdown m_playercount;
    public Toggle m_rankedMatch;


    public void LoadLevel(int level)
    {
        Application.LoadLevel(level);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenServerlist()
    {
        m_serverPanel.SetActive(true);
        m_mainMenuPanel.SetActive(false);
    }

    public void BackFromServers()
    {
        m_serverPanel.SetActive(false);
        m_mainMenuPanel.SetActive(true);
    }

    public void OpenHostMenu()
    {
        m_mainMenuPanel.SetActive(false);
        m_hostGamePanel.SetActive(true);
    }

    public void BackFromHosting()
    {
        m_mainMenuPanel.SetActive(true);
        m_hostGamePanel.SetActive(false);
    }

    public void RefreshServerlist()
    {
        //TODO: Implement refreshing
    }

    public void HostGame()
    {
        string serverName = m_serverName.text;
        string otherInfo = m_otherInput.text;
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

        


        //TODO: Implement game hosting
    }

}
