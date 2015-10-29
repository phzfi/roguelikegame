using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SyncTransform : NetworkBehaviour
{

    [SerializeField]
    Transform myTransform;
    [SyncVar]
    Vector3 syncPosition;
    [SyncVar]
    Quaternion syncRotation;

    [SerializeField]
    float syncRate = .5f;
    private float lastSync = -99.0f;

    void Update()
    {
        InterpolateTransform();
        syncClients();
    }

    [Server]
    void syncClients()
    {
        if (Time.realtimeSinceStartup - lastSync > syncRate)
        {
            lastSync = Time.realtimeSinceStartup;
            var clientObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (var obj in clientObjects)
            {
                var syncer = obj.GetComponent<SyncTransform>();
                if (syncer == null)
                    continue;
                syncer.RpcSyncClientObject();
            }
        }
    }

    void InterpolateTransform()
    {
        if(!isLocalPlayer)
        {
            myTransform.position = syncPosition; //don't actually interpolate for now
            myTransform.rotation = syncRotation;
        }
    }

    [Command]
    void CmdSendPositionToServer(Vector3 pos)
    {
        syncPosition = pos;
    }

    [Command]
    void CmdSendRotationToServer(Quaternion rot)
    {
        syncRotation = rot;
    }

    [ClientRpc]
    void RpcSyncClientObject()
    {
        CmdSendPositionToServer(myTransform.position);
        CmdSendRotationToServer(myTransform.rotation);
    }
}
