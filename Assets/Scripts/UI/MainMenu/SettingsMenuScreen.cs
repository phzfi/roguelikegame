using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SettingsMenuScreen : MenuScreen
{
	public MainMenuScreen m_mainMenu;

	public InputField m_nameInput;
	public Slider m_mainVolumeSlider;
	public Slider m_musicVolumeSlider;
	public Slider m_effectsVolumeSlider;
	public Slider m_uiVolumeSlider;

	protected override void Start()
	{
		base.Start();
		LoadSettings();
	}

	public override void Show()
	{
		LoadSettings();
		base.Show();
	}

	public override void Hide()
	{
		base.Hide();
		GlobalSettings.SaveToDisk();
	}

	public void BackButtonPressed()
	{
		Hide();
		m_mainMenu.Show();
	}

	public void PlayerNameChanged(string name)
	{
		GlobalSettings.playerName.Set(name);
	}

	public void MainVolumeChanged(float value)
	{
		GlobalSettings.mainAudioVolume.Set(value);
	}

	public void MusicVolumeChanged(float value)
	{
		GlobalSettings.musicAudioVolume.Set(value);
	}

	public void EffectsVolumeChanged(float value)
	{
		GlobalSettings.effectsAudioVolume.Set(value);
	}

	public void UIVolumeChanged(float value)
	{
		GlobalSettings.uiAudioVolume.Set(value);
	}

	private void LoadSettings()
	{
		m_nameInput.text = GlobalSettings.playerName;
		m_mainVolumeSlider.value = GlobalSettings.mainAudioVolume;
		m_musicVolumeSlider.value = GlobalSettings.musicAudioVolume;
		m_effectsVolumeSlider.value = GlobalSettings.effectsAudioVolume;
		m_uiVolumeSlider.value = GlobalSettings.uiAudioVolume;
	}

}