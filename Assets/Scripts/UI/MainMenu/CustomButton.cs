using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Button))]
public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
	public void OnPointerClick(PointerEventData eventData)
	{
		AudioManager.Instance.PlayButtonClickSound();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		AudioManager.Instance.PlayButtonHoverSound();
	}
}
