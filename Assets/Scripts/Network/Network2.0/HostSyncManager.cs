using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class HostSyncManager : NetworkBehaviour
{

    private float m_lastSync = -99.0f;
    public float m_syncRate = 2.0f;

    private Dictionary<int, HostPlayerData> m_stacks = new Dictionary<int, HostPlayerData>();
    private int playerIdCounter = 0;

    List<List<IOutputOrder>> m_tmpOutputOrders = new List<List<IOutputOrder>>();

    void Start ()
    {
        for (int i = 0; i < NetworkingUtilities.GetOutputNetworkMsgTypeCount(); ++i)
            m_tmpOutputOrders.Add(new List<IOutputOrder>());
    }
	
	void Update ()
    {
        if (Time.realtimeSinceStartup - m_lastSync > m_syncRate) // start turn change if enough time has passed since last turn, and we're on the server
        {
            //TODO GAME LOGIC
            m_lastSync = Time.realtimeSinceStartup;
            SendOutputStacks();
        }
    }

    void SendOutputStacks()
    {
        foreach(var key in m_stacks.Keys)
        {
            HostPlayerData data = m_stacks[key];
            List<IOutputOrder> outputStack = data.m_outputStack;

            for (int i = 0; i < outputStack.Count; ++i)
            {
                IOutputOrder order = outputStack[i];
                order.SetTurnQueuePosition(i);
                networkingMsgType msgType = outputStack[i].GetMessageType();
                int index = NetworkingUtilities.GetOutputNetworkMsgTypeIndex(msgType);
                m_tmpOutputOrders[index].Add(order);
            }
            outputStack.Clear();
            SendOutputMessages(data.m_connectionId);
            ClearTmpLists();
        }
    }

    void SendOutputMessages(int connectionId)
    {
        {
            VisibilityOutputMessage msg = new VisibilityOutputMessage();
            msg.SetOrders(m_tmpOutputOrders[0]);
            NetworkServer.SendToClient(connectionId, (short)networkingMsgType.visibilityOutputMessage, msg);
        }
    }

    void ClearTmpLists()
    {
        for (int i = 0; i < m_tmpOutputOrders.Count; ++i)
            m_tmpOutputOrders[i].Clear();
    }

    public void InitOnServer() // initialize server side sync logic
    {
        NetworkServer.RegisterHandler((short)networkingMsgType.connectPlayer, ConnectNewPlayer);
        NetworkServer.RegisterHandler((short)networkingMsgType.inputMovementMessage, HandleInputMessage);
        NetworkServer.RegisterHandler((short)networkingMsgType.visibilityOutputMessage, ErrorMessageHandler);
        enabled = true;
        Debug.Log("Initialized at server");

        playerIdCounter = 0;
        m_stacks.Clear();

        //TODO Actual player count
        GameLogic.Instance.Initialize(4);
    }

    public void StopOnServer()
    {
        NetworkServer.UnregisterHandler((short)networkingMsgType.connectPlayer);
        NetworkServer.UnregisterHandler((short)networkingMsgType.inputMovementMessage);
        NetworkServer.UnregisterHandler((short)networkingMsgType.visibilityOutputMessage);
    }

    void ConnectNewPlayer(NetworkMessage msg)
    {
        var connectMessage = msg.ReadMessage<InputConnectMessage>();
        int playerId = connectMessage.m_playerIndex;
        int connectionId = connectMessage.m_connectionIndex;
        if (playerId == -1)
        {
            HostPlayerData stackData = new HostPlayerData(connectionId);
            m_stacks.Add(playerIdCounter, stackData);
            VisibilityOutputOrder visibilityOrder = new VisibilityOutputOrder();
            var outputMsg = GameLogic.Instance.PlayerAdd(++playerIdCounter, ref visibilityOrder);
            stackData.m_outputStack.Add(visibilityOrder);

            NetworkServer.SendToClient(connectionId, (short)networkingMsgType.connectPlayer, outputMsg);
            Debug.Log("Send initialized at client to server");
        }
        else
        {
            if (!m_stacks.ContainsKey(playerId))
                Debug.LogError("Reconnecting player with invalid id");
            m_stacks[playerId].Clear();
            //DummyGameLogic.Instance.ReconnectPlayer(playerId);
        }
    }

    public void HandleInputMessage(NetworkMessage msg)
    {
        //TODO 
    }

    public void ErrorMessageHandler(NetworkMessage msg)
    {
        Debug.LogError("Host syncmanager got invalid message");
    }
}
