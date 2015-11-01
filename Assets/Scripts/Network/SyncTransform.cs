using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SyncTransform : NetworkBehaviour
{
    public static List<GameObject> players;
    
    [SerializeField]
    float syncRate = .5f;

    private float lastSync = -99.0f;
    
    void Start()
    {
        players = new List<GameObject>();
    }

    void Update()
    {
        if(isServer)
            syncClients();
    }

    [Server]
    void syncClients()
    {
        if (Time.realtimeSinceStartup - lastSync > syncRate)
        {
            lastSync = Time.realtimeSinceStartup;
            //var clientObjects = GameObject.FindGameObjectsWithTag("Player"); // find all network objects (not the smartest way maybe)
            foreach (var obj in players)
            {
                var syncer = obj.GetComponent<PlayerSync>();
                if (syncer == null)
                    continue;
                syncer.RpcSyncClientObject(); // call synchronization on remote client for all player objects
            }
        }
    }


}
