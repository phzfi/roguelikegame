using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public enum networkMsgType : short
{
    connectPlayer = 100,
    //Inout messages
    inputMessage = 101,
    //Output messages
    visibilityOutputMessage = 102
}

public class HostPlayerData
{
    public HostPlayerData(int connectionId)
    {
        m_connectionId = connectionId;
        m_inputStack = new List<System.Object> ();
        m_outputStack = new List<IOutputOrder> ();
    }

    public void Clear()
    {
        ClearInput();
        ClearOutput();
    }

    public void ClearInput()
    {
        m_inputStack.Clear();
    }

    public void ClearOutput()
    {
        m_outputStack.Clear();
    }

    public int m_connectionId;
    public List<System.Object> m_inputStack;
    public List<IOutputOrder> m_outputStack;
}

public class ClientPlayerData
{
    public int m_clientID;
    public NetworkConnection m_connection;
}