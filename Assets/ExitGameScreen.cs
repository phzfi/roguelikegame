using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(CustomNetworkManager))]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ExitGameScreen : NetworkBehaviour {

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
    

    public void OpenExitGamePanel()
    {
        m_exitGame.SetActive(true);
        m_volumeSlider.value = AudioListener.volume;
        sm_exitingGame = true;
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
        m_manager.StopHost();
        m_mainMenu.SetActive(true);
        m_exitGame.SetActive(false);
    }

}
