﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerSync : NetworkBehaviour {

    [SerializeField]
    SimpleCharacterMovement m_mover;

    [SyncVar]
    Vector2i m_syncPosition;
    
    void Start ()
    {
        SyncManager.sm_players.Add(this); // add this to list of player objects
	}
	
	void Update ()
    {
        m_mover.m_gridPos = m_syncPosition;
    }
    
    void OnDestroy()
    {
        SyncManager.sm_players.Remove(this); // remove this from list of player objects
    }
    
    public void SyncPosition(Vector2i pos)
    {
        m_syncPosition = pos;
    }

    public bool IsLocalPlayer()
    {
        return isLocalPlayer;
    }

    [Server]
    public void TakeServerTurn()  // runs the server-side turn logic for this object
    {
    }
}
