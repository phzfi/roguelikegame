using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ExitSettingsScreen : MenuScreen
{
	public GameObject m_exitMenu;

	public Slider m_mainVolumeSlider;
	public Slider m_musicVolumeSlider;
	public Slider m_effectsVolumeSlider;
	public Slider m_uiVolumeSlider;
	public Button m_applyButton;
	
	private float mainVolume;
	private float musicVolume;
	private float effectsVolume;
	private float uiAudioVolume;

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
		m_exitMenu.SetActive(true);
	}

	public void BackButtonPressed()
	{
		Hide();
	}

	public void ApplyButtonPressed()
	{
		ApplySettings();
	}
	
	public void MainVolumeChanged(float value)
	{
		mainVolume = value;
		ActivateApplyButton();
	}

	public void MusicVolumeChanged(float value)
	{
		musicVolume = value;
		ActivateApplyButton();
	}

	public void EffectsVolumeChanged(float value)
	{
		effectsVolume = value;
		ActivateApplyButton();
	}

	public void UIVolumeChanged(float value)
	{
		uiAudioVolume = value;
		ActivateApplyButton();
	}

	private void LoadSettings()
	{
		m_mainVolumeSlider.value = GlobalSettings.mainAudioVolume;
		m_musicVolumeSlider.value = GlobalSettings.musicAudioVolume;
		m_effectsVolumeSlider.value = GlobalSettings.effectsAudioVolume;
		m_uiVolumeSlider.value = GlobalSettings.uiAudioVolume;
		
		mainVolume = GlobalSettings.mainAudioVolume;
		musicVolume = GlobalSettings.musicAudioVolume;
		effectsVolume = GlobalSettings.effectsAudioVolume;
		uiAudioVolume = GlobalSettings.uiAudioVolume;

		DeactivateApplyButton();
	}

	private void ApplySettings()
	{
		GlobalSettings.mainAudioVolume.Set(mainVolume);
		GlobalSettings.musicAudioVolume.Set(musicVolume);
		GlobalSettings.effectsAudioVolume.Set(effectsVolume);
		GlobalSettings.uiAudioVolume.Set(uiAudioVolume);

		GlobalSettings.SaveToDisk();

		DeactivateApplyButton();
	}

	private void ActivateApplyButton()
	{
		m_applyButton.interactable = true;
	}

	private void DeactivateApplyButton()
	{
		m_applyButton.interactable = false;
	}

}
