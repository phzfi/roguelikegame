using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CreditsMenuScreen : MenuScreen, IPointerClickHandler
{
	public MainMenuScreen m_mainMenu;

	private int clickCount = 0;

	public override void Show()
	{
		base.Show();
		clickCount = 0;
	}

	public override void Hide()
	{
		base.Hide();
		GetComponent<CreditRandomizer>().Reset();
	}

	public void BackButtonPressed()
	{
		Hide();
		m_mainMenu.Show();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		clickCount++;
		if (clickCount == 3)
		{
			GetComponent<CreditRandomizer>().Shuffle();
		}
	}

}
