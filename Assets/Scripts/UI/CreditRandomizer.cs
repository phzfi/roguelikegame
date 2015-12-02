using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CreditRandomizer : MonoBehaviour
{

    public string[] m_names;
    public List<Text> m_creditTexts;

    void Start()
    {
        RandomizeNames();
    }


    public void RandomizeNames()
    {
        for (int i = 0; i < m_names.Length; i++)
        {
            int index = Random.Range(0, m_creditTexts.Count);
            m_creditTexts[index].text = m_names[i];
            m_creditTexts.RemoveAt(index);
        }
    }

}