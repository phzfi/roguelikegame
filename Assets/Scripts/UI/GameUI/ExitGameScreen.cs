using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ExitGameScreen : MonoBehaviour
{
	public ExitSettingsScreen m_exitSettings;
	public GameObject m_exitGame;
	public GameObject m_background;

    public GameObject m_victoryTextPrefab;
    public GameObject m_gameOverTextPrefab;

    private GameObject m_victoryText;
    private GameObject m_gameoverText;

    public static bool sm_exitMenuOpen = false;
	
    void Start()
    {
        if (m_victoryTextPrefab != null)
        {
            m_victoryText = GameObject.Instantiate(m_victoryTextPrefab);
            m_victoryText.transform.SetParent(transform, false);
            m_victoryText.SetActive(false);
        }
        else
            Debug.Log("Missing m_victoryTextPrefab in ExitGameScreen");

        if (m_gameOverTextPrefab != null)
        {
            m_gameoverText = GameObject.Instantiate(m_gameOverTextPrefab);
            m_gameoverText.transform.SetParent(transform, false);
            m_gameoverText.SetActive(false);
        }
        else
            Debug.Log("Missing m_gameOverTextPrefab in ExitGameScreen");

        gameObject.SetActive(false);
    }

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

    public void SetEndGameText(bool isVictory)
    {
        if (isVictory && m_victoryText != null)
            m_victoryText.SetActive(true);
        else if(m_gameoverText != null)
            m_gameoverText.SetActive(true);
    }
}
