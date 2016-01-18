using UnityEngine;
using System.Collections;

// Inherit this class and implement m_chaseRange and TakeTurn() to create new AI behaviour.
public abstract class EnemyAI : MonoBehaviour {
	public int m_chaseRange = 15;
	protected CharController m_controller;
	protected SimpleCharacterMovement m_mover;

	public void Start()
	{
		m_controller = GetComponent<CharController>();
		m_mover = m_controller.m_mover;
	}

	public abstract void TakeTurn(); // Runs the turn decision logic for this NPC. 
	
}
