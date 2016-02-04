using UnityEngine;
using System.Collections;

public class MainMenuScreen : MenuScreen
{
	public HostMenuScreen m_hostMenu;
	public JoinMenuScreen m_joinMenu;
	public LobbyMenuScreen m_lobbyMenu;
	public SettingsMenuScreen m_settingsMenu;
	public CreditsMenuScreen m_creditsMenu;
	public MenuScreen m_background;
	public MenuScreen m_blackBackground;

	public override void Show()
	{
		base.Show();
		m_background.Show();
    }

	public void HostButtonPressed()
	{
		Hide();
		m_hostMenu.Show();
	}

	public void JoinButtonPressed()
	{
		Hide();
		m_joinMenu.Show();
	}

	public void SettingsButtonPressed()
	{
		Hide();
		m_settingsMenu.Show();
	}

	public void CreditsButtonPressed()
	{
		Hide();
		m_creditsMenu.Show();
	}

	public void ExitButtonPressed()
	{
		Application.Quit();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

	// Used to fade out entire main menu when transitioning to game
	public void HideAll()
	{
		sm_overrideFadeInDuration = 0.5f;
		sm_overrideFadeOutDuration = 0.5f;

		Hide();
		m_hostMenu.Hide();
		m_joinMenu.Hide();
		m_lobbyMenu.Hide();
		m_settingsMenu.Hide();
		m_creditsMenu.Hide();
		m_background.Hide();

		sm_overrideFadeInDuration = -1.0f;
		sm_overrideFadeOutDuration = -1.0f;
	}
}
