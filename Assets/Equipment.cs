using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Equipment : NetworkBehaviour {

    
    public static List<GameObject> sm_equipment;

    void Start()
    {
        sm_equipment = new List<GameObject>();
    }



}
