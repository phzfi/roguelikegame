using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class MenuScreen : MonoBehaviour
{
	protected CanvasGroup m_canvasGroup;

	protected float m_fadeInDuration = 0.1f;
	protected float m_fadeOutDuration = 0.1f;

	protected float m_fadeInWait = 0.0f;
	protected float m_fadeOutWait = 0.0f;

	protected Button[] m_buttons = new Button[0];

	protected static float sm_overrideFadeInDuration = -1.0f;
	protected static float sm_overrideFadeOutDuration = -1.0f;

	virtual protected void Start()
	{
		// Use canvas group alpha to show/hide menu
		// instead of gameobject active/deactive.
		// This way animations work correctly after Hide and Show
		m_canvasGroup = GetComponent<CanvasGroup>();
		m_buttons = GetComponentsInChildren<Button>();
	}

	virtual public void Show()
	{
		m_canvasGroup.blocksRaycasts = true;

		StopAllCoroutines();

		float fadeInDuration = m_fadeInDuration;
		if (sm_overrideFadeInDuration >= 0.0f)
			fadeInDuration = sm_overrideFadeInDuration;

		if (fadeInDuration > 0.0f)
		{
			StartCoroutine(FadeAlphaCoroutine(m_canvasGroup.alpha, 1.0f, fadeInDuration, m_fadeInWait));
		}
		else
		{
			m_canvasGroup.alpha = 1.0f;
		}
	}

	virtual public void Hide()
	{
		m_canvasGroup.blocksRaycasts = false;

		StopAllCoroutines();

		float fadeOutDuration = m_fadeOutDuration;
		if (sm_overrideFadeOutDuration >= 0.0f)
			fadeOutDuration = sm_overrideFadeOutDuration;

		if (fadeOutDuration > 0.0f)
		{
			StartCoroutine(FadeAlphaCoroutine(m_canvasGroup.alpha, 0.0f, fadeOutDuration, m_fadeOutWait));
		}
		else
		{
			m_canvasGroup.alpha = 0.0f;
		}
	}

	IEnumerator FadeAlphaCoroutine(float from, float to, float duration, float wait)
	{
		if (wait > 0.0f)
			yield return new WaitForSeconds(wait);

		for (float time = 0.0f; time <= duration; time += Time.deltaTime)
		{
			float t = time / duration;
			m_canvasGroup.alpha = Mathf.Lerp(from, to, t);
			yield return null;
		}
		m_canvasGroup.alpha = to;
	}

	protected void SetButtonsEnabled(bool enabled)
	{
		for (int i = 0; i < m_buttons.Length; ++i)
		{
			m_buttons[i].interactable = enabled;
		}
	}

}
