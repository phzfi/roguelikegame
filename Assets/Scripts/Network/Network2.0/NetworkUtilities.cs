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
    public HostPlayerData(int connectionId)
    {
        m_connectionId = connectionId;
        m_inputStack = new List<InputOrder>();
    }

    public void Clear()
    {
        m_inputStack.Clear();
    }

    public int m_connectionId;
    public List<InputOrder> m_inputStack;
}

public class ClientPlayerData
{
    public int m_clientID;
    public NetworkConnection m_connection;
}