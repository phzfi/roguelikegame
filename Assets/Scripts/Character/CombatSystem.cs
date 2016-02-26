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
    public bool isBoss = false;
	public Text m_textPrefab;
    public AttackStyle m_currentAttackStyle = AttackStyle.MELEE;
    public AudioClip m_meleeAudio;
    public List<AudioClip> m_barehandSounds;
    public AudioClip m_mageAudio;
    public AudioClip m_rangedAudio;
    public List<AudioClip> m_deathSounds;
    public float m_attackSoundOffset = 0f;

	private GameObject m_textCanvas;
	private Text m_label;
	private Camera m_camera;
	private Inventory m_inventory;
    private Equipment m_equipment;
	private CharController m_controller;
    private AudioSource m_audioSource;
    private CharacterAnimation m_animator;

    public enum AttackStyle { MELEE, MAGE, RANGED };

	public void Start()
	{
		m_currentHp = m_maxHp;
		m_textCanvas = GameObject.FindGameObjectWithTag("TextCanvas");
        m_camera = Camera.main;
		m_label = Instantiate(m_textPrefab);
		m_label.transform.SetParent(m_textCanvas.transform, true);
		m_inventory = GetComponent<Inventory>(); // store inventory reference for use in damage modifiers
		m_controller = GetComponent<CharController>();
        m_equipment = GetComponent<Equipment>();
        m_audioSource = GetComponent<AudioSource>();
        m_animator = GetComponent<CharacterAnimation>();
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
	}

	public void Attack(int targetID) // Deal damage to object, identified by ID.
	{
		var target = CharManager.GetObject(targetID);
		var targetSystem = target.GetComponent<CombatSystem>();
		if (targetSystem == null)
			return;
        Invoke("PlayAttackSound", m_attackSoundOffset);
        m_animator.TriggerAttackAnimation();     
		targetSystem.ChangeHP(-GetDamage());
	}



	public void ChangeHP(int amount)
	{
		int actualAmount = amount;
		if(amount < 0)
			actualAmount = -GetReducedDamage(Mathf.Abs(amount)); // Damage resistance only applies if damage is being dealt.

		m_currentHp = Mathf.Min(m_maxHp, m_currentHp + actualAmount);
		if (m_currentHp <= 0)
			SyncManager.AddDeathOrder(m_controller.ID);
	}

	public void Die()
	{
        if (isBoss)
        {
            Debug.Log("boss killed!!"); // TODO: victory sequence
        } 
        
		m_controller.Unregister();
		m_label.enabled = false;
        m_animator.TriggerDeathAnimation();
        m_audioSource.PlayOneShot(m_deathSounds[Random.Range(0, m_deathSounds.Count)]);
        Invoke("Disable", 5f);
		Debug.Log("player killed");
	}

    void Disable()
    {
        gameObject.SetActive(false);
    }
}
