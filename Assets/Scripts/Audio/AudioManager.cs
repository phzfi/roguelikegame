using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
	//public List<AudioClip> m_musicList;
	public AudioClip m_menuMusic;
	public AudioClip m_ambientSounds;
	public AudioSource m_source;

	//private float m_menuVolume = 1.0f;
	//private float m_ambientVolume = 0.0f;
	private bool m_ambientPlaying = false;
	//private int m_currentSong = -1;

	void Start()
	{
		m_source.clip = m_menuMusic;
	}

	IEnumerator Wait()
	{
		yield return new WaitForSeconds(5.0f);
	}

	void Update()
	{
		if (!m_ambientPlaying && !MenuManager.sm_menuOpen)
		{
			m_ambientPlaying = true;
			StartCoroutine("FadeOut");
			StartCoroutine("Wait");
			m_source.clip = m_ambientSounds;
			StartCoroutine("FadeIn");
			m_source.Play();
		}
	}

	IEnumerator FadeOut()
	{
		for (float f = 1f; f > 0f; f -= 0.1f * Time.deltaTime)
		{
			m_source.volume = f;
			yield return null;
		}
	}

	IEnumerator FadeIn()
	{
		for (float f = 0f; f < 1.0f; f += 0.1f * Time.deltaTime)
		{
			m_source.volume = f;
			yield return null;
		}
	}
}
