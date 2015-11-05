using UnityEngine;
using System.Collections;

public class JoinMenuScreen : MonoBehaviour
{
    public GameObject m_serverPanel;
    public GameObject m_mainMenuPanel;

    public void JoinGame()
    {
        //TODO: Implement game joining
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
