using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangedAttack : MonoBehaviour {

	public float m_maxRange;
	public int m_damage;
	public float m_velocity;
	public ActionDelegate m_useDelegate;
    public AudioClip m_rangedSound;
	public GameObject m_projectilePrefab;
	
    private AudioSource m_audioSource;
	private Action m_projectileAction;
	public Action m_attackAction, m_damageVisualizeAction;

	public void Start()
	{
		m_attackAction = gameObject.AddComponent<Action>();
		m_attackAction.Initialize();
        m_attackAction.m_useDelegate = AttackAction;

		m_damageVisualizeAction = gameObject.AddComponent<Action>();
		m_damageVisualizeAction.Initialize();
		m_damageVisualizeAction.m_useDelegate = VisualizeDealtDamage;

		m_audioSource = GetComponent<AudioSource>();

		if(m_projectilePrefab != null)
		{
			m_projectileAction = gameObject.AddComponent<Action>();
			m_projectileAction.Initialize();
			m_projectileAction.m_useDelegate = StartVisualization;
		}
	}

	public void Attack(CharController target, CharController source, ref List<ActionData> visualization, ActionTargetData targetData)
	{
		if (target.m_mover.m_gridPos.Distance(source.m_mover.m_gridPos) > m_maxRange)
			return;

		if (!LineOfSight.CheckLOS(source.m_mover.m_navAgent, source.m_mover.m_gridPos, target.m_mover.m_gridPos, m_maxRange).blocked) // Check that target is visible
		{
			m_audioSource.PlayOneShot(m_rangedSound);

			if (m_projectilePrefab != null)
			{
				ActionTargetData facingTarget = new ActionTargetData();
				facingTarget.m_gridTarget = target.m_mover.m_gridPos;
				ActionData facingData = new ActionData();
				facingData.m_actionID = source.m_mover.m_turnToFaceAction.ID;
				facingData.m_target = facingTarget;
				visualization.Add(facingData);

				ActionData actionData = new ActionData();
				actionData.m_actionID = m_projectileAction.ID;
				actionData.m_target = targetData;
				visualization.Add(actionData);
			}

			ActionData damageData = new ActionData();
			damageData.m_actionID = m_damageVisualizeAction.ID;
			damageData.m_target = targetData;
			visualization.Add(damageData);

			target.m_combatSystem.TakeDamage(-m_damage, ref visualization);
		}
	}

	public void AttackAction(ActionTargetData targetData)
	{
		CharController target = null;
		CharController source = CharManager.GetObject(targetData.m_userID);

		if (targetData.m_playerTarget)
		{
			target = CharManager.GetObject(targetData.m_targetID);
		}
		else
		{
			for(int i = 0; i < CharManager.Objects.Count; ++i)
			{
				var controller = CharManager.Objects[i];
				if(controller.m_mover.m_gridPos == targetData.m_gridTarget)
				{
					target = controller;
					break;
				}
			}
		}

		if (target == null || source == null || target == source) // If target not found or attacker invalid, do nothing
			return;

		List<ActionData> actions = new List<ActionData>();
		Attack(target, source, ref actions, targetData);

		for(int i = 0; i < actions.Count; ++i)
		{
			var action = actions[i];
			SyncManager.AddVisualizeAction(action);
		}
	}

	public void StartVisualization(ActionTargetData targetData)
	{
		Debug.Log("visualizing ranged attack, ID: " + targetData.m_userID);
		Vector3 startPoint = CharManager.GetObject(targetData.m_userID).transform.position;
		Vector3 endPoint = MapGrid.GridToWorldPoint(targetData.m_gridTarget) - new Vector3(0,0,.3f);
		//Vector3 endPoint = CharManager.GetObject(targetData.m_targetID).transform.position;

		var projectile = Instantiate(m_projectilePrefab);
		projectile.transform.position = startPoint;
		var projectileScript = projectile.AddComponent<Projectile>();
		projectileScript.FireProjectile(startPoint, endPoint, m_velocity);
	}

	public void VisualizeDealtDamage(ActionTargetData data)
	{
		var target = CharManager.GetObject(data.m_targetID);
		var targetSystem = target.GetComponent<CombatSystem>();
		if (targetSystem == null)
			return;

		targetSystem.m_visualizeHp = Mathf.Max(0, targetSystem.m_visualizeHp - targetSystem.GetReducedDamage(m_damage));
		ClientTurnLogicManager.MarkActionFinished();
	}
}
