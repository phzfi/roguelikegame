using UnityEngine;
using System.Collections;

// Inherit this class and implement m_chaseRange and TakeTurn() to create new AI behaviour.
public abstract class EnemyAI : MonoBehaviour {
	public abstract int m_chaseRange;
	private CharController m_controller;
	private SimpleCharacterMovement m_mover;

	public void Start()
	{
		m_controller = GetComponent<CharController>();
		m_mover = m_controller.m_mover;
	}

	public abstract void TakeTurn(); // Runs the turn decision logic for this NPC. 
	
}
