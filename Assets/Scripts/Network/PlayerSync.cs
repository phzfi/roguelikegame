using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerSync : NetworkBehaviour {

    [SerializeField]
    SimpleCharacterMovement mover;

    [SyncVar]
    NavPath.GridPosition syncPosition;

    // Use this for initialization
    void Start ()
    {
        SyncTransform.players.Add(gameObject);
	}
	
	// Update is called once per frame
	void Update ()
    {
        InterpolateTransform();
    }
    
    void OnDestroy()
    {
        SyncTransform.players.Remove(gameObject);
    }

    void InterpolateTransform()
    {
        if (!isLocalPlayer)
        {
            mover.pos = syncPosition;
        }
    }

    [Command]
    void CmdSendPositionToServer(NavPath.GridPosition pos)
    {
        syncPosition = pos;
        //Debug.Log("Synced pos :" + pos + ", time = " + lastSync);
    }

    [ClientRpc]
    public void RpcSyncClientObject()
    {
        if (!isLocalPlayer)
            return;
        mover.TakeStep();
        CmdSendPositionToServer(mover.pos);
    }
}
