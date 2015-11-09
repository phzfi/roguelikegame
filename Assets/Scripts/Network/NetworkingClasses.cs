using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public enum msgType : short { moveOrder = 100, pickupOrder, connected, visualize, spawnItem } // start higher since unity reserves some message types

public struct MoveOrder
{
    public Vector3 m_target;
    public int m_moverID;
    public int m_turnNumber;

    public MoveOrder(Vector3 target, int moverID)
    {
        m_turnNumber = SyncManager.sm_currentTurn;
        m_moverID = moverID;
        m_target = target;
    }
}


public class ConnectionMessage : MessageBase
{
    public uint m_clientID;
}

public class MoveOrderMessage : MessageBase
{
    public MoveOrder[] m_orders;
    public uint m_clientID;
}

public struct PickupOrder
{
    public int m_playerID, m_itemID, m_turnNumber;
    public PickupOrder(int playerID, int itemID)
    {
        m_playerID = playerID;
        m_itemID = itemID;
        m_turnNumber = SyncManager.sm_currentTurn;
    }
}

public class PickupOrderMessage : MessageBase
{
    public PickupOrder[] m_orders;
}

public struct SpawnOrder
{
    public string m_prefabName;
    public int m_ID;
    public Vector2i m_position;
}

public class SpawnOrderMessage : MessageBase
{
    public SpawnOrder[] m_orders;
}

public class ClientData
{
    public bool m_turnInProgress;
    public bool m_receivedInput;
    public uint m_clientID;
    public NetworkConnection m_connection;
}

public class PlayerData
{
    public uint m_clientID;
    public int m_connectionID;
    public NetworkConnection m_connection;
    public bool m_receivedInput;
}

public class ServerData
{
    public List<PlayerData> m_playerData = new List<PlayerData>(); 
    public bool m_turnInProgress;
}
