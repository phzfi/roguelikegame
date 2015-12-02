using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SettingsMenuScreen : MonoBehaviour
{

    public GameObject m_settingsPanel;
    public GameObject m_mainMenuPanel;
    public GameObject m_creditsPanel;

    public void CloseSettings()
    {
        m_settingsPanel.SetActive(false);
        m_mainMenuPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        m_creditsPanel.SetActive(true);
        m_settingsPanel.SetActive(false);
    }

    public void CloseCredits()
    {
        m_creditsPanel.SetActive(false);
        m_settingsPanel.SetActive(true);
    }

}