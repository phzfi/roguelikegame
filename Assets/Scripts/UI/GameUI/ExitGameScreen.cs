using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ExitGameScreen : MonoBehaviour
{
	public GameObject m_options;
	public GameObject m_exitGame;
	public Slider m_volumeSlider;
	
	public static bool sm_exitMenuOpen = false;
	
	public void ToggleExitGamePanel()
	{
		if (!sm_exitMenuOpen)
		{
			m_exitGame.SetActive(true);
			m_volumeSlider.value = AudioListener.volume;
			sm_exitMenuOpen = true;
		}
		else
		{
			m_exitGame.SetActive(false);
			sm_exitMenuOpen = false;
		}
	}

	public void ContinueGame()
	{
		m_exitGame.SetActive(false);
		sm_exitMenuOpen = false;
	}

	public void OpenSettings()
	{
		m_options.SetActive(true);
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
		}

		m_exitGame.SetActive(false);
	}

}
