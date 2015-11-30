using UnityEngine;
using System.Collections;

public class MainMenuScreen : MonoBehaviour
{
	public GameObject m_serverPanel;
	public GameObject m_mainMenuPanel;
	public GameObject m_hostGamePanel;

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
}
