using UnityEngine;
using System.Collections;

public class MainMenuScreen : MonoBehaviour
{
	public GameObject m_serverPanel;
	public GameObject m_mainMenuPanel;
	public GameObject m_hostGamePanel;
    public GameObject m_settingsPanel;

	public void OpenJoinMenu()
	{
		m_serverPanel.SetActive(true);
		m_mainMenuPanel.SetActive(false);
	}

	public void OpenHostMenu()
	{
		m_mainMenuPanel.SetActive(false);
		m_hostGamePanel.SetActive(true);
	}

    public void OpenSettings()
    {
        m_settingsPanel.SetActive(true);
        m_mainMenuPanel.SetActive(false);
    }
}
