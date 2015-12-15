using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CharController : NetworkBehaviour
{
	[HideInInspector]
	public int ID;
	[HideInInspector]
	public SimpleCharacterMovement m_mover = null; // Store references to avoid having to use GetComponent everywhere
	[HideInInspector]
	public PlayerSync m_syncer = null;
	[HideInInspector]
	public Inventory m_inventory = null;
	[HideInInspector]
	public Equipment m_equipment = null;
	[HideInInspector]
	public EnemyAI m_enemyAI = null; // is null if player character

	public bool m_isPlayer; // Whether this character is a player or an enemy NPC

	// Use this for initialization
	void Start ()
	{
		ID = (int)netId.Value;
		CharManager.Register(this);
		m_syncer = GetComponent<PlayerSync>();
		m_mover = GetComponent<SimpleCharacterMovement>();
		m_inventory = GetComponent<Inventory>();
		m_equipment = GetComponent<Equipment>();

		if(!m_isPlayer)
			m_enemyAI = GetComponent<EnemyAI>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void OnDestroy()
	{
		Unregister();
	}

	public void Unregister()
	{
		CharManager.Unregister(ID);
	}
}
