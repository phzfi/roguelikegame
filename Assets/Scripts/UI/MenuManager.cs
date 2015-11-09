using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static bool sm_menuOpen = true;
    public GameObject m_mainMenu;
    public GameObject m_hostMenu;
    public GameObject m_joinMenu;
    public AudioClip m_hoverAudio;
    public AudioClip m_clickAudio;

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
    }

    public void HostGameFromHostMenu()
    {
        m_hostMenu.GetComponent<HostMenuScreen>().HostGame();
        sm_menuOpen = false;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
