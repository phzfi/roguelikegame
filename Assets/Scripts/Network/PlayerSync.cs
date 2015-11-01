using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerSync : NetworkBehaviour {

    [SerializeField]
    SimpleCharacterMovement m_mover;

    [SyncVar]
    NavPath.GridPosition m_syncPosition;
    
    void Start ()
    {

        SyncTransform.sm_players.Add(gameObject); // add this to list of player objects
	}
	
	void Update ()
    {
        InterpolateTransform();
    }
    
    void OnDestroy()
    {
        SyncTransform.sm_players.Remove(gameObject); // remove this from list of player objects
    }

    public void MoveOrder(Vector3 target) // called from input handler, run pathfinding locally and tell server to run it too if not currently server
    {
        if (!isLocalPlayer)
            return;

        m_mover.MoveTo(target);
        if(!isServer)
        {
            CmdRunServerPathFinding(target);
        }
    }

    [Server]
    public void TakeTurn()  // runs the server-side turn logic for this object
    {
        bool moved = m_mover.TakeStep();
        m_syncPosition = m_mover.pos;

        if(!isLocalPlayer)
            RpcTookStep(moved);
    }

    [ClientRpc]
    void RpcTookStep(bool moved) // tell client a step was taken
    {
        if (!isLocalPlayer)
            return;
        
        if(m_mover.pathScript._path.Count > 0 && moved) // remove next segment from pathfinding path if moved this turn
            m_mover.pathScript._path.RemoveAt(0);
    }

    [Command]
    void CmdRunServerPathFinding(Vector3 target)
    {
        m_mover.MoveTo(target);
    }

    void InterpolateTransform() // read synced position 
    {
        m_mover.pos = m_syncPosition;
    }
}
