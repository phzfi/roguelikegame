using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public enum networkMsgType : short
{
    connectPlayer = 100
}

public class HostPlayerData
{
    public HostPlayerData(NetworkConnection connection)
    {
        m_connection = connection;
        m_inputStack = new List<InputOrder>();
    }

    public void Clear()
    {
        m_inputStack.Clear();
    }

    public NetworkConnection m_connection;
    public List<InputOrder> m_inputStack;
}

public class ClientPlayerData
{
    public int m_clientID;
    public NetworkConnection m_connection;
}