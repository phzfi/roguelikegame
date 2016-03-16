using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasScaler))]
public class UIScaler : MonoBehaviour
{
	private CanvasScaler m_scaler = null;

	void Awake()
	{
		m_scaler = GetComponent<CanvasScaler>();
		UpdateScaling();
	}

	void Start()
	{
		if (m_scaler.uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize)
		{
			Debug.LogError("UI Scaler is meant to be used with constant pixel size scaling!");
		}
		UpdateScaling();
	}

	void LateUpdate()
	{
		UpdateScaling();
	}

	private void UpdateScaling()
	{
		// Constant pixel size works otherwise well but for UHD resolutions (1440p+) we want some upscale
		m_scaler.scaleFactor = Mathf.Max(Mathf.Max(Screen.width / 1920.0f, Screen.height / 1080.0f), 1.0f);
	}
}
