using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SyncTransform : NetworkBehaviour
{
    public static List<GameObject> sm_players;
    public float m_syncRate = .5f;

    private float m_lastSync = -99.0f;
    
    void Start()
    {
        sm_players = new List<GameObject>();
    }

    void Update()
    {
        if(isServer)
            SyncClients();
    }

    [Server]
    void SyncClients()
    {
        if (Time.realtimeSinceStartup - m_lastSync > m_syncRate)
        {
            m_lastSync = Time.realtimeSinceStartup;
            for (int i = 0; i < sm_players.Count; ++i)
            {
                var obj = sm_players[i];
                var syncer = obj.GetComponent<PlayerSync>();
                if (syncer == null)
                    continue;
                syncer.RpcSyncClientObject(); // call synchronization on remote client for all player objects
            }
        }
    }


}
