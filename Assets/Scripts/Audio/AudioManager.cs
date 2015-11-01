using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {

    public List<AudioClip> m_musicList;
    public AudioSource m_source;

    private int m_currentSong = -1;
    
	void Start () {
        
	}
	
	void Update () {
        if (m_source.isPlaying)
            return;
	    for(int i = 0; i < m_musicList.Count; ++i)
        {
            if (i == m_currentSong)
                continue;

            var clip = m_musicList[i];
            m_source.clip = clip;
            m_source.Play();
            m_currentSong = i;
            break;
        }
	}
}
