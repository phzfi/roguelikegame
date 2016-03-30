using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatSystem : NetworkBehaviour
{
	[SyncVar]
	public int m_currentHp;
	public int m_visualizeHp;
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
	public Action m_attackVisualizeAction, m_deathVisualizeAction, m_damageVisualizeAction, m_healVisualizeAction;

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
		m_visualizeHp = m_currentHp;
		m_textCanvas = GameObject.FindGameObjectWithTag("TextCanvas");
        m_camera = Camera.main;
		m_label = Instantiate(m_textPrefab);
		m_label.transform.SetParent(m_textCanvas.transform, true);
		m_inventory = GetComponent<Inventory>(); // store inventory reference for use in damage modifiers
		m_controller = GetComponent<CharController>();
        m_equipment = GetComponent<Equipment>();
        m_audioSource = GetComponent<AudioSource>();
        m_animator = GetComponent<CharacterAnimation>();

		var actionpool = GetComponent<ActionPool>();
		m_attackVisualizeAction = gameObject.AddComponent<Action>();
		m_attackVisualizeAction.Initialize();
		m_attackVisualizeAction.m_useDelegate = VisualizeAttack;
		m_deathVisualizeAction = gameObject.AddComponent<Action>();
		m_deathVisualizeAction.Initialize();
		m_deathVisualizeAction.m_useDelegate = VisualizeDeath;
		m_damageVisualizeAction = gameObject.AddComponent<Action>();
		m_damageVisualizeAction.Initialize();
		m_damageVisualizeAction.m_useDelegate = VisualizeDealtDamage;
		m_healVisualizeAction = gameObject.AddComponent<Action>();
		m_healVisualizeAction.Initialize();
		m_healVisualizeAction.m_useDelegate = VisualizeHeal;

	}

	public void Update()
	{
		m_label.text = m_visualizeHp + "/" + m_maxHp;
		m_label.transform.position = m_camera.WorldToScreenPoint(gameObject.transform.position);
	}

	private int GetDamage() // Get the attack damage of this object, modified by weapons etc.
	{
        var actualDamage = GetReducedDamage(m_damage + Mathf.CeilToInt(m_equipment.m_playerStrength * 0.25f));
		return actualDamage;
	}

	public int GetReducedDamage(int incomingDamage) // Modify incoming attack damage by damage reduction from armor
	{
        var actualDamage = incomingDamage - Mathf.CeilToInt(0.25f * m_equipment.m_playerVitality);
		return Mathf.Clamp(actualDamage, 0, incomingDamage);
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

	public void Attack(int targetID, ref List<ActionData> visualization) // Deal damage to object, identified by ID.
	{
		var target = CharManager.GetObject(targetID);
		var targetSystem = target.GetComponent<CombatSystem>();
		if (targetSystem == null)
			return;

		ActionTargetData targetData = new ActionTargetData(); // Add visualization order for hp reduction
		targetData.m_targetID = targetID;

		ActionData data = new ActionData();
		data.m_actionID = m_damageVisualizeAction.ID;
		data.m_target = targetData;
		visualization.Add(data);

		data = new ActionData(); // And attack animation
		data.m_actionID = m_attackVisualizeAction.ID;
		visualization.Add(data);

		targetSystem.TakeDamage(-GetDamage(), ref visualization);
	}

	public void VisualizeDealtDamage(ActionTargetData data)
	{
		var target = CharManager.GetObject(data.m_targetID);
		var targetSystem = target.GetComponent<CombatSystem>();
		if (targetSystem == null)
			return;

		targetSystem.m_visualizeHp = Mathf.Max(0, targetSystem.m_visualizeHp - targetSystem.GetReducedDamage(GetDamage()));
		ClientTurnLogicManager.MarkActionFinished();
	}

	public void VisualizeHeal(ActionTargetData data)
	{
		int heal = data.m_gridTarget.x; // TODO: potato solution. Make something better.
		m_visualizeHp = Mathf.Min(m_maxHp, m_visualizeHp + heal);
		ClientTurnLogicManager.MarkActionFinished();
	}

	public void VisualizeAttack(ActionTargetData data)
	{
		Debug.Log("visualizing attack, id: " + m_controller.ID);
		Invoke("PlayAttackSound", m_attackSoundOffset);
		StartCoroutine(PlayAnimationCoRoutine(AnimationType.attack));
	}

	public IEnumerator PlayAnimationCoRoutine(AnimationType type)
	{
		float startTime = 0;
		m_animator.TriggerAnimation(type);
		while (true)
		{
			if (m_animator.IsAnimationPlaying(type)) // First wait for animation to start
			{
				startTime = Time.realtimeSinceStartup;
				break;
			}
			yield return null;
		}

		while (true)
		{
			if (type == AnimationType.attack)
			{
				if (!m_animator.IsAnimationPlaying(type)) // Then wait until it is finished
				{
					ClientTurnLogicManager.MarkActionFinished();
					yield break;
				}
				yield return null;
			}
			else
			{
				if (Time.realtimeSinceStartup - startTime > 3.0f)
				{
					ClientTurnLogicManager.MarkActionFinished();
					Invoke("Disable", 2);
					yield break;
				}
				yield return null;
			}
		}
	}

	public void Heal(int amount, ref List<ActionData> visualization)
	{
		m_currentHp = Mathf.Min(m_maxHp, m_currentHp + amount);

		ActionData action = new ActionData();
		action.m_actionID = m_healVisualizeAction.ID;
		ActionTargetData target = new ActionTargetData();
		target.m_gridTarget = new Vector2i(amount, 0);
		target.m_targetID = m_controller.ID;
		action.m_target = target;
		visualization.Add(action);
	}

	public void TakeDamage(int amount, ref List<ActionData> visualization)
	{
		int actualAmount = GetReducedDamage(Mathf.Abs(amount));

		m_currentHp = m_currentHp - actualAmount;
		if (m_currentHp <= 0)
        {
            m_currentHp = 0;

			ActionData action = new ActionData();
			action.m_actionID = m_deathVisualizeAction.ID;
			visualization.Add(action);
        }
			
	}

	public void VisualizeDeath(ActionTargetData data)
	{
        if (isBoss)
        {
            Debug.Log("boss killed!!"); // TODO: victory sequence
        } 
        
		m_controller.Unregister();
		m_label.enabled = false;
		Debug.Log("visualizing death, id: " + m_controller.ID);
		m_audioSource.PlayOneShot(m_deathSounds[Random.Range(0, m_deathSounds.Count)]);
		StartCoroutine(PlayAnimationCoRoutine(AnimationType.death));
	}

    void Disable()
    {
        gameObject.SetActive(false);
    }
}
