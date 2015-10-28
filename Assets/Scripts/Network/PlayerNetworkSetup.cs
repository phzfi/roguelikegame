﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerNetworkSetup : NetworkBehaviour
{

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
        {
            GetComponent<SimpleCharacterMovement>().enabled = true;
        }
    }
}
