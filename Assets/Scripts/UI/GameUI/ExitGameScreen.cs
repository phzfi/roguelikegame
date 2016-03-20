using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ExitGameScreen : MonoBehaviour
{
	public ExitSettingsScreen m_exitSettings;
	public GameObject m_exitGame;
	public GameObject m_background;
	
	public static bool sm_exitMenuOpen = false;
	
	public void ToggleExitGamePanel()
	{
		if (!sm_exitMenuOpen)
		{
			m_background.SetActive(true);
			m_exitGame.SetActive(true);
			sm_exitMenuOpen = true;
		}
		else
		{
			m_background.SetActive(false);
			m_exitGame.SetActive(false);
			sm_exitMenuOpen = false;
		}
	}

	public void ContinueGame()
	{
        m_background.SetActive(false);
        m_exitGame.SetActive(false);
		sm_exitMenuOpen = false;
	}

	public void OpenSettings()
	{
		m_exitSettings.Show();
		gameObject.SetActive(false);
	}

	public void ExitGame()
	{
		sm_exitMenuOpen = false;

		LobbyManager lobbyManager = FindObjectOfType<LobbyManager>();

		if (lobbyManager != null)
		{
			lobbyManager.ExitGame();
		}
		else
		{
			Application.Quit();
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

		m_exitGame.SetActive(false);
		m_background.SetActive(false);
	}

}
