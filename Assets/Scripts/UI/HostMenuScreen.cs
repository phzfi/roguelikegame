using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HostMenuScreen : MonoBehaviour
{
    public GameObject m_mainMenuPanel;
    public GameObject m_hostGamePanel;

    public InputField m_serverName;
    public InputField m_otherInput;
    public Dropdown m_playercount;
    public Toggle m_rankedMatch;

    public void CloseHostMenu()
    {
        m_mainMenuPanel.SetActive(true);
        m_hostGamePanel.SetActive(false);
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
