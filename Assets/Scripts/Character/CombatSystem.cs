using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatSystem : NetworkBehaviour
{
	[SyncVar]
	public int m_currentHp;
	public int m_maxHp = 3;
	public int m_damage = 1;
	public Text m_textPrefab;
    public AttackStyle m_currentAttackStyle = AttackStyle.MELEE;
    public AudioClip m_meleeAudio;
    public List<AudioClip> m_barehandSounds;
    public AudioClip m_mageAudio;
    public AudioClip m_rangedAudio;

    private GameObject m_textCanvas;
	private Text m_label;
	private Camera m_camera;
	private Inventory m_inventory;
    private Equipment m_equipment;
	private CharController m_controller;
    private AudioSource m_audioSource;


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
        m_equipment = GetComponent<Equipment>();
        m_audioSource = GetComponent<AudioSource>();
	}

	public void Update()
	{
		m_label.text = m_currentHp + "/" + m_maxHp;
		m_label.transform.position = m_camera.WorldToScreenPoint(gameObject.transform.position);
	}

	private int GetDamage() // Get the attack damage of this object, modified by weapons etc.
	{
        var actualDamage = GetReducedDamage(m_damage + Mathf.CeilToInt(m_equipment.m_playerStrength * 0.25f));
		return actualDamage; // TODO: damage modifiers
	}

	private int GetReducedDamage(int incomingDamage) // Modify incoming attack damage by damage reduction from armor
	{
        var actualDamage = incomingDamage - Mathf.CeilToInt(0.25f * m_equipment.m_playerVitality);
		return actualDamage; // TODO: damage reduction
	}

    private void PlayAttackSound()
    {
        switch (m_currentAttackStyle)
        {
            case AttackStyle.MELEE:
                var equipment = m_equipment.m_equipment;
                bool swordEquipped = false;
                for (int i = 0; i < equipment.Count; ++i)
                {
                    if(equipment[i].GetComponent<Item>().m_name.Contains("Sword"))
                    {
                        swordEquipped = true;
                    }
                    continue;
                }
                if (!swordEquipped)
                    m_audioSource.PlayOneShot(m_barehandSounds[Random.Range(0, m_barehandSounds.Count - 1)]);
                else
                    m_audioSource.PlayOneShot(m_meleeAudio);
                break;
            case AttackStyle.MAGE:
                m_audioSource.PlayOneShot(m_mageAudio);
                break;
            case AttackStyle.RANGED:
                m_audioSource.PlayOneShot(m_rangedAudio);
                break;
        }
    }

    public void Attack(int targetID) // Deal damage to object, identified by ID.
	{
		var target = CharManager.GetObject(targetID);
		var targetSystem = target.GetComponent<CombatSystem>();
		if (targetSystem == null)
			return;
        PlayAttackSound();        
		targetSystem.GetHit(GetDamage());
	}

    

	public void GetHit(int dmg)
	{
		int takenDamage = GetReducedDamage(dmg);
		m_currentHp -= takenDamage;
		if (m_currentHp <= 0)
			SyncManager.AddDeathOrder(m_controller.ID);
	}

	public void Die()
	{
		m_controller.Unregister();
		m_label.enabled = false;
		gameObject.SetActive(false);
		Debug.Log("player killed");
	}
}
