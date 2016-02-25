using UnityEngine;
using System.Collections;

public class CharacterAnimation : MonoBehaviour
{
    //TODO: Fix animations
    private Animator m_animator;

    void Start()
    {
        m_animator = GetComponent<Animator>();
    }
	
    public void ToggleWalkAnimation(bool on)
    {
        m_animator.SetBool("IsWalking", on);
        if (on) Debug.Log("Walking turned on.");
        else Debug.Log("Walking turned off.");
    }


}
