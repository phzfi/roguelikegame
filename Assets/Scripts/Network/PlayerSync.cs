using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerSync : NetworkBehaviour {

    [SerializeField]
    SimpleCharacterMovement m_mover;

    [SyncVar]
    NavPath.GridPosition m_syncPosition;

    // Use this for initialization
    void Start ()
    {
        SyncTransform.sm_players.Add(gameObject);
	}
	
	// Update is called once per frame
	void Update ()
    {
        InterpolateTransform();
    }
    
    void OnDestroy()
    {
        SyncTransform.sm_players.Remove(gameObject);
    }

    void InterpolateTransform()
    {
        if (!isLocalPlayer)
        {
            m_mover.pos = m_syncPosition;
        }
    }

    [Command]
    void CmdSendPositionToServer(NavPath.GridPosition pos)
    {
        m_syncPosition = pos;
        //Debug.Log("Synced pos :" + pos + ", time = " + lastSync);
    }

    [ClientRpc]
    public void RpcSyncClientObject()
    {
        if (!isLocalPlayer)
            return;
        m_mover.TakeStep();
        CmdSendPositionToServer(m_mover.pos);
    }
}
