using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Equipment : NetworkBehaviour
{
    
    public List<GameObject> m_equipment = new List<GameObject>();

    [SyncVar]
    public int m_playerStrength = 0;
    [SyncVar]
    public int m_playerAgility = 0;
    [SyncVar]
    public int m_playerIntelligence = 0;
    [SyncVar]
    public int m_playerVitality = 0;


}
