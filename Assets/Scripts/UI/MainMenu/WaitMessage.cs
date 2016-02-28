using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaitMessage : MenuScreen
{
	public Text m_messageText;

	private System.Action m_onCancelAction;

	protected override void Start()
	{
		m_fadeInDuration = 0.5f;
		m_fadeInWait = 0.5f;
		base.Start();
	}

	public override void Show()
	{
		m_messageText.text = "Connecting...";
		m_onCancelAction = null;
		base.Show();
	}

	public void Show(string message, System.Action cancelAction)
	{
		m_messageText.text = message;
		m_onCancelAction = cancelAction;
		base.Show();
	}

	public void CancelButtonPressed()
	{
		if (m_onCancelAction != null)
			m_onCancelAction();

		Hide();
	}
}
