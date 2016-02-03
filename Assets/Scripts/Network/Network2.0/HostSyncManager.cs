using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class HostSyncManager : NetworkBehaviour
{

    private float m_lastSync = -99.0f;
    public float m_syncRate = 2.0f;

    private Dictionary<int, HostPlayerData> InputStacks = new Dictionary<int, HostPlayerData>();
    private int playerIdCounter = 0;

    void Start ()
    {
	}
	
	void Update ()
    {
        if (Time.realtimeSinceStartup - m_lastSync > m_syncRate) // start turn change if enough time has passed since last turn, and we're on the server
        {
            //TODO GAME LOGIC
            m_lastSync = Time.realtimeSinceStartup;
        }
    }

    public void InitOnServer() // initialize server side sync logic
    {
        NetworkServer.RegisterHandler((short)networkMsgType.connectPlayer, ConnectNewPlayer);
        enabled = true;
        Debug.Log("Initialized at server");

        playerIdCounter = 0;
        InputStacks.Clear();
        DummyGameLogic.Instance.Inititialize();
    }

    public void StopOnServer()
    {
        NetworkServer.UnregisterHandler((short)networkMsgType.connectPlayer);
    }

    void ConnectNewPlayer(NetworkMessage msg)
    {
        var connectMessage = msg.ReadMessage<InputConnectMessage>();
        int playerId = connectMessage.m_playerIndex;
        var connection = connectMessage.m_connection;
        if (playerId == -1)
        {
            InputStacks.Add(playerIdCounter, new HostPlayerData(connection));
            playerIdCounter++;
            DummyGameLogic.Instance.AddPlayer();

            //var outputMsg = new OutputConnectMessage();
            //connection.Send((short)networkMsgType.connectPlayer, outputMsg);
            //Debug.Log("Send initialized at client to server");
        }
        else
        {
            if (!InputStacks.ContainsKey(playerId))
                Debug.LogError("Reconnecting player with invalid id");
            InputStacks[playerId].Clear();
            DummyGameLogic.Instance.ReconnectPlayer(playerId);
        }
    }
}
