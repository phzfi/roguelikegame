using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CombatSystem : NetworkBehaviour
{
	[SyncVar]
	public int m_currentHp;
	public int m_maxHp = 3;
	public int m_damage = 1;
	public Text m_textPrefab;

	private Canvas m_textCanvas;
	private Text m_label;
	private Camera m_camera;

	public void Start()
	{
		m_currentHp = m_maxHp;
		m_textCanvas = GameObject.FindGameObjectWithTag("TextCanvas").GetComponent<Canvas>();
		m_camera = FindObjectOfType<Camera>();
		m_label = Instantiate(m_textPrefab);
		m_label.transform.SetParent(m_textCanvas.transform, true);
		//m_label.transform.parent = m_textCanvas.transform;
	}

	public void Update()
	{
		m_label.text = m_currentHp + "/" + m_maxHp;

		m_label.transform.position = m_camera.WorldToScreenPoint(gameObject.transform.position);
	}

	public void Attack(int targetID)
	{
		var target = MovementManager.GetObject(targetID);
		var targetSystem = target.GetComponent<CombatSystem>();
		if (targetSystem == null)
			return;

		targetSystem.GetHit(m_damage);
	}

	public void GetHit(int dmg)
	{
		m_currentHp -= dmg;
		if (m_currentHp <= 0)
			Die();
	}

	public void Die()
	{
		//gameObject.SetActive(false);
	}
}
