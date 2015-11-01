using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SyncTransform : NetworkBehaviour
{
    [SerializeField]
    SimpleCharacterMovement mover;

    [SyncVar]
    NavPath.GridPosition syncPosition;

    [SerializeField]
    float syncRate = .5f;

    private float lastSync = -99.0f;
    

    void Update()
    {
        InterpolateTransform();
        if(isServer)
            syncClients();
    }

    [Server]
    void syncClients()
    {
        if (Time.realtimeSinceStartup - lastSync > syncRate)
        {
            lastSync = Time.realtimeSinceStartup;
            var clientObjects = GameObject.FindGameObjectsWithTag("Player"); // find all network objects (not the smartest way maybe)
            foreach (var obj in clientObjects)
            {
                var syncer = obj.GetComponent<SyncTransform>();
                if (syncer == null)
                    continue;
                syncer.RpcSyncClientObject(); // call synchronization on remote client for all player objects
            }
        }
    }

    void InterpolateTransform()
    {
        if (!isLocalPlayer)
        {
            mover.pos = syncPosition;
            //Debug.Log("Synced pos :" + mover.pos + ", time = " + lastSync);
        }
    }

    [Command]
    void CmdSendPositionToServer(NavPath.GridPosition pos)
    {
        syncPosition = pos;
        //Debug.Log("Synced pos :" + pos + ", time = " + lastSync);
    }

    [ClientRpc]
    void RpcSyncClientObject()
    {
        if (!isLocalPlayer)
            return;
        mover.TakeStep();
        CmdSendPositionToServer(mover.pos);
    }
}
