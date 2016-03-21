using UnityEngine;
using System.Collections;

public class CharacterAnimation : MonoBehaviour
{
    //TODO: Fix animations
    private Animator m_animator;
	private AnimationStatus[] m_animationStatus = new AnimationStatus[(int)AnimationType.size];

    void Start()
    {
        m_animator = GetComponent<Animator>();

		var animations = m_animator.GetBehaviours<AnimationStatus>(); // Get all animation status behaviours for this animator
		Debug.Assert(animations.Length == (int)AnimationType.size);
		for(int i = 0; i < animations.Length; ++i)
		{
			m_animationStatus[(int)animations[i].m_type] = animations[i]; // Assign each animation to their correct slot
		}
	}
	
    public void ToggleWalkAnimation(bool on)
    {
        m_animator.SetBool("IsWalking", on);
    }

    public void TriggerAnimation(AnimationType type)
    {
		if(type == AnimationType.attack)
			m_animator.SetTrigger("Attack");
		else if(type == AnimationType.death)
			m_animator.SetTrigger("Death");
	}

	public bool IsAnimationPlaying(AnimationType type)
	{
		return m_animationStatus[(int)type].m_animationRunning;
	}
}
