using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(LayoutElement))]
public class ServerLabel : MonoBehaviour
{
	public JoinMenuScreen m_joinMenu;
	public Text m_nameText;
	public Text m_addressText;
	public Text m_playersText;
	public InputField m_directConnectAddress;
	public InputField m_directConnectPort;

	private int m_index = -1;
	private CanvasGroup m_canvasGroup;
	private LayoutElement m_layoutElement;
	private bool m_visible = false;

	public void Awake()
	{
		m_canvasGroup = GetComponent<CanvasGroup>();
		m_layoutElement = GetComponent<LayoutElement>();
	}

	public void Show()
	{
		m_canvasGroup.alpha = 1.0f;
		m_canvasGroup.blocksRaycasts = true;
		m_layoutElement.ignoreLayout = false;
		m_visible = true;
	}

	public void Hide()
	{
		m_canvasGroup.alpha = 0.0f;
		m_canvasGroup.blocksRaycasts = false;
		m_layoutElement.ignoreLayout = true;
		m_visible = false;
	}

	public bool IsVisible()
	{
		return m_visible;
	}

	public void SetIndex(int index)
	{
		m_index = index;
	}

	public int GetIndex()
	{
		return m_index;
	}

	public void UpdateTexts(string serverName, string address, int port, int players, int maxPlayers)
	{
		if (m_nameText)
		{
			TextGenerator textGen = new TextGenerator();
			var settings = m_nameText.GetGenerationSettings(Vector2.zero);
			float width = textGen.GetPreferredWidth(serverName, settings);
			
			bool bShortened = false;
			while (serverName.Length > 5 && textGen.GetPreferredWidth(serverName, settings) > 175)
			{
				serverName = serverName.Substring(0, serverName.Length - 1);
				bShortened = true;
			}

			m_nameText.text = bShortened ? serverName + "..." : serverName;
		}
		

		if (m_addressText)
			m_addressText.text = string.Format("({0}:{1})", address, port);

		if (m_playersText)
			m_playersText.text = string.Format("{0} / {1}", players, maxPlayers);

		if (m_directConnectAddress)
			m_directConnectAddress.text = address;

		if (m_directConnectPort)
			m_directConnectPort.text = port.ToString();
	}

	public void OnSelected(bool selected)
	{
		Debug.Assert(m_index >= 0, "Trying to select server label without index!");

		if (selected)
		{
			m_joinMenu.SelectServer(m_index);
		}
		else
		{
			m_joinMenu.SelectServer(-1);
		}
	}
}
