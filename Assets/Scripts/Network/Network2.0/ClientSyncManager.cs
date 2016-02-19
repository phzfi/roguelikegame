using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ClientSyncManager : NetworkBehaviour
{

    private ClientPlayerData m_playerData;
    private LevelMapManager m_levelManager;
    private List<IOutputOrder> m_outputOrderStack = new List<IOutputOrder>();

    private bool m_initialized = false;

    void Update()
    {
        if (!m_initialized)
            return;

        for (int i = 0; i < m_outputOrderStack.Count; ++i)
        {
            IOutputOrder order = (IOutputOrder) m_outputOrderStack[i];
            order.ExecuteOrder(this);
        }
        m_outputOrderStack.Clear();

    }

    public void InitOnClient(NetworkConnection connection) // initialize client side sync logic
    {
        m_levelManager = FindObjectOfType<LevelMapManager>();

        m_playerData = new ClientPlayerData();
        m_playerData.m_connection = connection;
        m_playerData.m_clientID = -1;
        m_playerData.m_connection.RegisterHandler((short)networkingMsgType.connectPlayer, HandleIntializion);
        m_playerData.m_connection.RegisterHandler((short)networkingMsgType.inputMovementMessage, ErrorMessageHandler);
        m_playerData.m_connection.RegisterHandler((short)networkingMsgType.visibilityOutputMessage, HandleVisibilityOutputMessage);

        var msg = new InputConnectMessage();
        msg.m_playerIndex = -1;
        msg.m_connectionIndex = connection.connectionId;
        m_playerData.m_connection.Send((short)networkingMsgType.connectPlayer, msg);
        Debug.Log("Send initialized at client to server");
    }

    public void StopOnClient()
    {
        m_playerData.m_connection.UnregisterHandler((short)networkingMsgType.connectPlayer);
        m_playerData.m_connection.UnregisterHandler((short)networkingMsgType.inputMovementMessage);
        m_playerData.m_connection.UnregisterHandler((short)networkingMsgType.visibilityOutputMessage);
        m_playerData = null;
        m_initialized = false;
    }

    public void HandleIntializion(NetworkMessage msg)
    {
        OutputConnectMessage message = msg.ReadMessage<OutputConnectMessage>();
        m_levelManager.Initialize(message.m_mapSize.x, message.m_mapSize.y);
        m_initialized = true;
    }

    public void HandleVisibilityOutputMessage(NetworkMessage msg)
    {
        VisibilityOutputMessage message = msg.ReadMessage<VisibilityOutputMessage>();
        message.SetOrdersToInterface<VisibilityOutputOrder>(ref m_outputOrderStack, message.m_orders);
    }

    public void ExecuteVisibilityOrder(VisibilityOutputOrder order)
    {
        for (int i = 0; i < order.m_numberOfTiles; ++i)
           m_levelManager.AddToMap(order.m_gridPostions[i], (MapTileType)order.m_mapTiles[i], order.m_visualizationIndices[i]);
        m_levelManager.Visualize();
    }

    public void ErrorMessageHandler(NetworkMessage msg)
    {
        Debug.LogError("Client syncmanager got invalid message");
    }

}
