using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SyncTransform : NetworkBehaviour
{

    [SerializeField]
    Transform myTransform;
    [SerializeField]
    float lerprate = 15;
    [SyncVar]
    Vector3 syncPosition;
    [SyncVar]
    Quaternion syncRotation;

    void FixedUpdate()
    {
        TransmitTransform();
        InterpolateTransform();
    }

    void InterpolateTransform()
    {
        if(!isLocalPlayer)
        {
            myTransform.position = Vector3.Lerp(myTransform.position, syncPosition, lerprate * Time.deltaTime);
            myTransform.rotation = Quaternion.Slerp(myTransform.rotation, syncRotation, lerprate * Time.deltaTime);
        }
    }

    [Command]
    void CmdSendPosition(Vector3 pos)
    {
        syncPosition = pos;
    }

    [Command]
    void CmdSendRotation(Quaternion rot)
    {
        syncRotation = rot;
    }

    [ClientCallback]
    void TransmitTransform()
    {
        if (isLocalPlayer)
        {
            CmdSendPosition(myTransform.position);
            CmdSendRotation(myTransform.rotation);
        }
    }
}
