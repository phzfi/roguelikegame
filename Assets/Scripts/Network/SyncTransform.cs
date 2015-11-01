using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SyncTransform : NetworkBehaviour
{
    public static List<GameObject> sm_players = new List<GameObject>();
    public float m_syncRate = .5f;

    private float m_lastSync = -99.0f;
    

    void Update()
    {
        if (isServer && Time.realtimeSinceStartup - m_lastSync > m_syncRate)
        {
            m_lastSync = Time.realtimeSinceStartup;
            RunTurn();
        }
    }

    [Server]
    void RunTurn() // advances the game state by one turn, runs all the server-side game logic
    {
        for (int i = 0; i < sm_players.Count; ++i)
        {
            var obj = sm_players[i];
            var syncer = obj.GetComponent<PlayerSync>();
            if (syncer == null)
                continue;

            syncer.TakeTurn();
        }
    }


}
