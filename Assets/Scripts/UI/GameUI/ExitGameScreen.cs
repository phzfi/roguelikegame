using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ExitGameScreen : MonoBehaviour
{
	public GameObject m_options;
	public GameObject m_mainMenu;
	public GameObject m_exitGame;
	public Slider m_volumeSlider;

	public GameObject m_networkManager;

	public static bool sm_exitingGame = false;

	private CustomNetworkManager m_manager;

	void Awake()
	{
		m_manager = m_networkManager.GetComponent<CustomNetworkManager>();
	}

	public void ToggleExitGamePanel()
	{
		if (!sm_exitingGame)
		{
			m_exitGame.SetActive(true);
			m_volumeSlider.value = AudioListener.volume;
			sm_exitingGame = true;
		}
		else
		{
			m_exitGame.SetActive(false);
			sm_exitingGame = false;
		}

	}

	public void ContinueGame()
	{
		m_exitGame.SetActive(false);
		sm_exitingGame = false;
	}

	public void OpenSettings()
	{
		m_options.SetActive(true);
		gameObject.SetActive(false);
	}

	public void ExitGame()
	{
		m_manager.StopClient();
		m_manager.StopHost(); //TODO: Not working
		m_mainMenu.SetActive(true);
		m_exitGame.SetActive(false);
	}

}
