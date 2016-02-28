using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ErrorMessage : MenuScreen
{
	public Text m_messageText;

	public override void Show()
	{
		m_messageText.text = "Error";
		base.Show();
	}

	public void Show(string message)
	{
		m_messageText.text = message;
		base.Show();
	}

	public void OKButtonPressed()
	{
		Hide();
	}
}
