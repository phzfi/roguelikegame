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
	public AttackStyle m_currentAttackStyle = AttackStyle.MELEE;
	public GameObject m_coinsPrefab;

	private GameObject m_textCanvas;
	private Text m_label;
	private Camera m_camera;
	private Inventory m_inventory;
	private CharController m_controller;



    public enum AttackStyle { MELEE, MAGE, RANGED };

	public void Start()
	{
		m_currentHp = m_maxHp;
		m_textCanvas = GameObject.FindGameObjectWithTag("TextCanvas");
		m_camera = FindObjectOfType<Camera>();
		m_label = Instantiate(m_textPrefab);
		m_label.transform.SetParent(m_textCanvas.transform, true);
		m_inventory = GetComponent<Inventory>(); // store inventory reference for use in damage modifiers
		m_controller = GetComponent<CharController>();
	}

	public void Update()
	{
		m_label.text = m_currentHp + "/" + m_maxHp;
		m_label.transform.position = m_camera.WorldToScreenPoint(gameObject.transform.position);
	}

	private int GetDamage() // Get the attack damage of this object, modified by weapons etc.
	{
		return m_damage; // TODO: damage modifiers
	}

	private int GetReducedDamage(int incomingDamage) // Modify incoming attack damage by damage reduction from armor
	{
		return incomingDamage; // TODO: damage reduction
	}

	public void Attack(int targetID) // Deal damage to object, identified by ID.
	{
		var target = CharManager.GetObject(targetID);
		var targetSystem = target.GetComponent<CombatSystem>();
		if (targetSystem == null)
			return;

		targetSystem.GetHit(GetDamage());
	}

	public void GetHit(int dmg)
	{
		int takenDamage = GetReducedDamage(dmg);
		m_currentHp -= takenDamage;
		if (m_currentHp <= 0) {
			SyncManager.AddDeathOrder(m_controller.ID);
		}
	}

	public void DropItems()
    {
        // TODO: Switch to ItemSpawners
        if (m_inventory.m_amountOfCoins > 0) {
			for (int i = 0; i < m_inventory.m_amountOfCoins; i++) {
				Vector3 pos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -1.0f);
				GameObject obj = (GameObject)Instantiate(m_coinsPrefab, pos, Quaternion.identity);
                if (obj == null) continue;
				var item = obj.GetComponent<Item>();
				
				ItemManager.GetID(out item.ID);
				item.m_pos = MapGrid.WorldToGridPoint(transform.position);
				
				NetworkServer.Spawn(obj);
			}
		}
        
        // TODO: Same here
        if (m_inventory.m_items.Count > 0)
        {
            for (int i = 0; i < m_inventory.m_items.Count; ++i)
            {
                Vector3 pos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -1.0f);
                GameObject obj = (GameObject) Instantiate(m_inventory.m_items[i], pos, Quaternion.identity);
                if (obj == null) continue;
                var item = obj.GetComponent<Item>();

                ItemManager.GetID(out item.ID);
                item.m_pos = MapGrid.WorldToGridPoint(transform.position);

                NetworkServer.Spawn(obj);
            }
        }
	}

	public void Die()
	{
		m_controller.Unregister();
		m_label.enabled = false;
		gameObject.SetActive(false);
		DropItems ();
		Debug.Log("player killed");
	}
}
