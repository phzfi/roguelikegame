using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Equipment : NetworkBehaviour {

    
    public static List<GameObject> sm_equipment;

    [SyncVar]
    public int m_playerStrength = 0;
    [SyncVar]
    public int m_playerAgility = 0;
    [SyncVar]
    public int m_playerIntelligence = 0;

    void Start()
    {
        sm_equipment = new List<GameObject>();
    }



}
