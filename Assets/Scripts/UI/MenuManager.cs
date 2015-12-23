using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;


public class MenuManager : MonoBehaviour
{
	public static bool sm_menuOpen = true;
	public GameObject m_mainMenu;
	public GameObject m_hostMenu;
	public GameObject m_joinMenu;
    public GameObject m_exitMenu;
    public GameObject m_exitMenuSettings;
	public AudioClip m_hoverAudio;
	public AudioClip m_clickAudio;
    public GameObject m_settings;
    public Slider m_volume;
    public GameObject m_actionBar;

    public void Start()
	{
		// Debug option to host game automatically
		if (!m_mainMenu.activeInHierarchy)
		{
			var manager = FindObjectOfType<NetworkManager>();
			if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null)
			{
				manager.StartHost();
                sm_menuOpen = false;
			}
		}
	}

	public void ShowJoinMenuFromMainMenu()
	{
		m_mainMenu.GetComponent<MainMenuScreen>().OpenJoinMenu();
	}

	public void ShowHostMenuFromMainMenu()
	{
		m_mainMenu.GetComponent<MainMenuScreen>().OpenHostMenu();
	}

	public void ShowMainMenuFromJoinMenu()
	{
		m_joinMenu.GetComponent<JoinMenuScreen>().CloseJoinMenu();
	}

	public void ShowMainMenuFromHostMenu()
	{
		m_hostMenu.GetComponent<HostMenuScreen>().CloseHostMenu();
	}

	public void RefreshServerList()
	{
		m_joinMenu.GetComponent<JoinMenuScreen>().RefreshGameList();
	}

	public void JoinGameFromServerlist()
	{
		m_joinMenu.GetComponent<JoinMenuScreen>().JoinGame();
		sm_menuOpen = false;
        m_actionBar.SetActive(true);
	}

	public void HostGameFromHostMenu()
	{
		m_hostMenu.GetComponent<HostMenuScreen>().HostGame();
		sm_menuOpen = false;
        m_actionBar.SetActive(true);
    }

	public void ExitGame()
	{
		Application.Quit();
	}

    public void ShowSettingsFromMainMenu()
    {
        m_mainMenu.GetComponent<MainMenuScreen>().OpenSettings();
    }

    public void ShowMainMenuFromSettings()
    {
        m_settings.GetComponent<SettingsMenuScreen>().CloseSettings();
    }

    public void ChangeVolume()
    {
        AudioListener.volume = m_volume.value;
    }

    public void OpenCredits()
    {
        m_settings.GetComponent<SettingsMenuScreen>().OpenCredits();
    }

    public void CloseCredits()
    {
        m_settings.GetComponent<SettingsMenuScreen>().CloseCredits();
    }

    public void OpenExitGame()
    {
        m_exitMenu.GetComponent<ExitGameScreen>().OpenExitGamePanel();
    }

    public void CloseExitGame()
    {
        m_exitMenu.GetComponent<ExitGameScreen>().ContinueGame();
    }

    public void OpenExitSettings()
    {
        m_exitMenu.GetComponent<ExitGameScreen>().OpenSettings();
    }

    public void CloseExitSettings()
    {
        m_exitMenuSettings.GetComponent<ExitSettingsScreen>().CloseExitSettings();
    }

    public void ReturnToMainMenu()
    {
        m_exitMenu.GetComponent<ExitGameScreen>().ExitGame();
    }
}
