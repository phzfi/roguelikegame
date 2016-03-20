using UnityEngine;
using System.Collections;

public class RangedAttack : MonoBehaviour {

	public float m_maxRange;
	public int m_damage;
	public float m_velocity;
	public ActionDelegate m_useDelegate;
    public AudioClip m_rangedSound;
	public GameObject m_projectilePrefab;
	
    private AudioSource m_audioSource;
	private Action m_projectileAction;

	public void Start()
	{
		m_useDelegate = new ActionDelegate(Attack);
		var action = GetComponent<Action>();
		action.m_useDelegate = m_useDelegate;
        m_audioSource = GetComponent<AudioSource>();

		if(m_projectilePrefab != null)
		{
			m_projectileAction = gameObject.AddComponent<Action>();
			m_projectileAction.Initialize();
			m_projectileAction.m_useDelegate = StartVisualization;
		}
	}

	public void Attack(ActionTargetData targetData)
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

		if (!LineOfSight.CheckLOS(source.m_mover.m_navAgent, source.m_mover.m_gridPos, target.m_mover.m_gridPos, m_maxRange).blocked) // Check that target is visible
		{
			target.m_combatSystem.ChangeHP(-m_damage);
            m_audioSource.PlayOneShot(m_rangedSound);

			if (m_projectilePrefab != null)
			{
				ActionTargetData facingTarget = new ActionTargetData();
				facingTarget.m_gridTarget = target.m_mover.m_gridPos;
				ActionData facingData = new ActionData();
				facingData.m_actionID = source.m_mover.m_turnToFaceAction.ID;
				facingData.m_target = facingTarget;
				SyncManager.AddVisualizeAction(facingData);

				ActionData actionData = new ActionData();
				actionData.m_actionID = m_projectileAction.ID;
				actionData.m_target = targetData;
				SyncManager.AddVisualizeAction(actionData);
            }
		}
	}

	public void StartVisualization(ActionTargetData targetData)
	{
		Vector3 startPoint = CharManager.GetObject(targetData.m_userID).transform.position;
		Vector3 endPoint = CharManager.GetObject(targetData.m_targetID).transform.position;

		var projectile = Instantiate(m_projectilePrefab);
		projectile.transform.position = startPoint;
		var projectileScript = projectile.AddComponent<Projectile>();
		projectileScript.FireProjectile(startPoint, endPoint, m_velocity);
	}	
}
