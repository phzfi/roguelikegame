using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public enum networkingMsgType : short
{
    connectPlayer = 100,
    inputMessagesBoundary = 101,
    inputMovementMessage = 102,
    outputMessagesBoundary = 103, 
    visibilityOutputMessage = 104,
    count = 105
}
class NetworkingUtilities
{
    public static int GetOutputNetworkMsgTypeIndex(networkingMsgType type)
    {
        Debug.Assert(type > networkingMsgType.outputMessagesBoundary);
        return (int)type - (int)networkingMsgType.outputMessagesBoundary - 1;
    }

    public static int GetOutputNetworkMsgTypeCount()
    {
        return (int)networkingMsgType.count - (int)networkingMsgType.inputMessagesBoundary;
    }

    public static int GetInputNetworkMsgTypeIndex(networkingMsgType type)
    {
        Debug.Assert(type > networkingMsgType.inputMessagesBoundary
            && type < networkingMsgType.outputMessagesBoundary);
        return (int)type - (int)networkingMsgType.inputMessagesBoundary - 1;
    }

    public static int GetInputNetworkMsgTypeCount()
    {
        return (int)networkingMsgType.outputMessagesBoundary - (int)networkingMsgType.inputMessagesBoundary;
    }
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