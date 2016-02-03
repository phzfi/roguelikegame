using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientSyncManager : NetworkBehaviour
{

    private ClientPlayerData m_playerData;

    public void InitOnClient(NetworkConnection connection) // initialize client side sync logic
    {
        m_playerData = new ClientPlayerData();
        m_playerData.m_connection = connection;
        m_playerData.m_clientID = -1;
        m_playerData.m_connection.RegisterHandler((short)networkMsgType.connectPlayer, HandleIntializion);

        var msg = new InputConnectMessage();
        msg.m_playerIndex = -1;
        msg.m_connection = connection;
        m_playerData.m_connection.Send((short)networkMsgType.connectPlayer, msg);
        Debug.Log("Send initialized at client to server");
    }

    public void StopOnClient()
    {
        m_playerData.m_connection.UnregisterHandler((short)msgType.moveOrder);
        m_playerData = null;
    }

    public void HandleIntializion(NetworkMessage msg)
    {
        Debug.Log("Client game logic initialization");
    }

    void EmptyMessageHandler(NetworkMessage msg)
    {
    }
}
