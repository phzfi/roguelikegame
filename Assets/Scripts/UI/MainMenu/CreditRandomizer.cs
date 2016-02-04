using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CreditRandomizer : MonoBehaviour
{
	public string[] m_names;
	public List<Text> m_creditTexts;

	private bool m_shuffle = false;

	void Start()
	{
		RandomizeNames();
	}

	public void RandomizeNames()
	{
		List<int> indices = new List<int>();
		for (int i = 0; i < m_names.Length; i++)
			indices.Add(i);

		for (int i = 0; i < m_names.Length; i++)
		{
			int index = Random.Range(0, indices.Count);
			m_creditTexts[indices[index]].text = m_names[i];
			indices.RemoveAt(index);
		}
	}

	public void Reset()
	{
		m_shuffle = false;

		for (int i = 0; i < m_creditTexts.Count; i++)
		{
			Vector3 localPos = m_creditTexts[i].transform.localPosition;
			localPos.x = 0.0f;
			m_creditTexts[i].transform.localPosition = localPos;
		}
	}

	public void Shuffle()
	{
		m_shuffle = true;
		for (int i = 0; i < m_creditTexts.Count; i++)
			StartCoroutine(ShuffleCoroutine(m_creditTexts[i].transform, 0.2f * i));
		Debug.Log("Every day I'm shuffling...");
	}

	IEnumerator ShuffleCoroutine(Transform t, float delay)
	{
		yield return new WaitForSeconds(delay);
		float time = 0.0f;
		while (m_shuffle)
		{
			Vector3 localPos = t.localPosition;
			localPos.x = 40.0f * Mathf.Sin(2.0f * time);
			t.localPosition = localPos;
			time += Time.smoothDeltaTime;
			yield return null;
		}
	}

}