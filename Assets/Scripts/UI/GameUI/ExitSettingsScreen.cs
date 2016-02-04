using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ExitSettingsScreen : MonoBehaviour
{
	public GameObject m_exitMenu;
	public GameObject m_exitSettings;
	
	public void CloseExitSettings()
	{
		m_exitMenu.SetActive(true);
		m_exitSettings.SetActive(false);
	}
	
}
