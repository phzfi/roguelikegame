using UnityEngine;
using System.Collections;

public class CharacterAnimation : MonoBehaviour
{
    //TODO: Fix animations
    private Animator m_animator;
	private AttackAnimation m_attackAnimation;

    void Start()
    {
        m_animator = GetComponent<Animator>();
		m_attackAnimation = m_animator.GetBehaviour<AttackAnimation>();
	}
	
    public void ToggleWalkAnimation(bool on)
    {
        m_animator.SetBool("IsWalking", on);
    }

    public void TriggerAttackAnimation()
    {
        m_animator.SetTrigger("Attack");
    }

    public void TriggerDeathAnimation()
    {
        m_animator.SetTrigger("Death");
    }

	public bool IsAttackPlaying()
	{
		return m_attackAnimation.m_attacking;
	}

}
