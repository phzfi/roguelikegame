using UnityEngine;
using System.Collections;

public class ActionBar : MonoBehaviour
{
	public UIManager m_uiManager;
	public ExitGameScreen m_exitMenu;

	public void InventoryButtonPressed()
	{
		m_uiManager.ToggleInventory();
	}

	public void EquipmentButtonPressed()
	{
		m_uiManager.ToggleEquipment();
	}

	public void ExitGameButtonPressed()
	{
		m_exitMenu.ToggleExitGamePanel();
	}

}
