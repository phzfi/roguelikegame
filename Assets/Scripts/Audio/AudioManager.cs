using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
	protected AudioManager() { }

	public AudioMixer m_audioMixer;
	public AudioSource m_musicAudioSource;
	public AudioSource m_menuAudioSource;

	public AudioClip m_buttonHover;
	public AudioClip m_buttonClick;
	public AudioClip m_menuMusic;
	public AudioClip m_ambientSounds;

	private bool m_ambientPlaying = false;

	void Start()
	{
		m_musicAudioSource.clip = m_menuMusic;
		m_musicAudioSource.volume = 0.0f;
		m_musicAudioSource.Play();
		StartCoroutine(FadeIn(2.0f));
	}

	void Update()
	{
		AudioListener.volume = GlobalSettings.mainAudioVolume;
		m_audioMixer.SetFloat("MusicVolume", VolumeToDecibels(GlobalSettings.musicAudioVolume));
		m_audioMixer.SetFloat("EffectsVolume", VolumeToDecibels(GlobalSettings.effectsAudioVolume));
		m_audioMixer.SetFloat("UIVolume", VolumeToDecibels(GlobalSettings.uiAudioVolume));

		if (Application.loadedLevelName == "MainMenu")
		{
			if (m_ambientPlaying)
			{
				m_ambientPlaying = false;
				StartCoroutine("FadeOut", 1.0f);
				StartCoroutine("Wait");
				m_musicAudioSource.clip = m_menuMusic;
				StartCoroutine("FadeIn", 1.0f);
				m_musicAudioSource.Play();
			}
		}
		else 
		{
			if (!m_ambientPlaying)
			{
				m_ambientPlaying = true;
				StartCoroutine("FadeOut", 1.0f);
				StartCoroutine("Wait");
				m_musicAudioSource.clip = m_ambientSounds;
				StartCoroutine("FadeIn", 1.0f);
				m_musicAudioSource.Play();
			}
		}
	}

	public void PlayButtonHoverSound()
	{
		m_menuAudioSource.PlayOneShot(m_buttonHover);
	}

	public void PlayButtonClickSound()
	{
		m_menuAudioSource.PlayOneShot(m_buttonClick);
	}

	private float VolumeToDecibels(float volume)
	{
		float dB = 20f * Mathf.Log10(volume * volume);
		if (float.IsInfinity(dB))
			dB = -80f;
		return dB;
	}

	private IEnumerator Wait()
	{
		yield return new WaitForSeconds(5.0f);
	}

	private IEnumerator FadeOut(float duration = 1.0f)
	{
		for (float t = duration; t >= 0.0f; t -= Time.deltaTime)
		{
			m_musicAudioSource.volume = t / duration;
			yield return null;
		}
		m_musicAudioSource.volume = 0.0f;
	}

	private IEnumerator FadeIn(float duration = 1.0f)
	{
		for (float t = 0.0f; t < duration; t += Time.deltaTime)
		{
			m_musicAudioSource.volume = t / duration;
			yield return null;
		}
		m_musicAudioSource.volume = 1.0f;
	}
}
