using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public enum msgType : short
{
	moveOrder = 100,
	pickupOrder,
	connected,
	visualize,
	spawnItem,
	attackOrder,
	death,
	turnSync,
    equipOrder,
	actionOrder
} // start higher since unity reserves some message types

public struct MoveOrder
{
	public Vector2i m_targetGridPos;
	public int m_moverID;
	public int m_turnNumber;

	public MoveOrder(Vector2i targetGridPos, int moverID)
	{
		m_turnNumber = SyncManager.sm_currentTurn;
		m_moverID = moverID;
		m_targetGridPos = targetGridPos;
	}

	public bool IsValid() // Check the validity of incoming move order
	{
		if (m_turnNumber != SyncManager.sm_currentTurn)
		{
			Debug.Log("invalid move order: turn number wrong!");
			return false;
		}

		var mover = CharManager.GetObject(m_moverID);
		if (mover == null)
		{
			Debug.Log("invalid move order: mover not found!");
			return false;
		}

		var path = mover.m_mover.m_navAgent.SeekPath(mover.m_mover.m_gridPos, m_targetGridPos);
		if (path.Count > mover.m_mover.m_gridSpeed || path[path.Count-1] != m_targetGridPos)
		{
			Debug.Log("invalid move order: invalid move target!");
			return false;
		}
		return true;
	}
}


public struct AttackOrder
{
	public int m_moverID;
	public int m_targetID;
	public int m_turnNumber;

	public AttackOrder(int moverID, int targetID)
	{
		m_moverID = moverID;
		m_targetID = targetID;
		m_turnNumber = SyncManager.sm_currentTurn;
	}

	public bool IsValid() // Check the validity of incoming attack order
	{
		if (m_turnNumber == SyncManager.sm_currentTurn)
			return true;
		else
		{
			Debug.Log("invalid attack order: turn number wrong!");
			return false;
		}
	}
}

public struct EquipOrder
{
    public bool m_equipType;
    public int m_itemID;
    public int m_playerID;

    public EquipOrder(bool equipType, int itemID, int playerID)
    {
        m_equipType = equipType;
        m_itemID = itemID;
        m_playerID = playerID;
    }
}

public class EquipOrderMessage : MessageBase
{
    public EquipOrder[] m_orders;
    public int m_clientID;
}

public class AttackOrderMessage : MessageBase
{
	public AttackOrder[] m_orders;
	public int m_clientID;
}

public class ConnectionMessage : MessageBase
{
	public int m_clientID;
}

public class MoveOrderMessage : MessageBase
{
	public MoveOrder[] m_orders;
	public int m_clientID;
}
public class ActionMessage : MessageBase
{
	public ActionData[] m_actions;
	public int m_clientID;
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

public struct DeathOrder
{
	public int m_targetID;
	public int m_turnNumber;

	public DeathOrder(int targetID)
	{
		m_targetID = targetID;
		m_turnNumber = SyncManager.sm_currentTurn;
	}
}

public class DeathMessage : MessageBase
{
	public DeathOrder[] m_orders;
	public uint m_clientID;
}

public class TurnSyncMessage : MessageBase
{
	public int m_turnNumber;
}

public class ClientData
{
	public bool m_turnInProgress;
	public bool m_receivedInput;
	public int m_clientID;
	public NetworkConnection m_connection;
}

public class PlayerData
{
	public int m_connectionID;
	public NetworkConnection m_connection;
	public bool m_receivedMoveInput;
	public bool m_receivedAttackInput;
	public bool m_receivedEquipInput;
}

public class ServerData
{
	public List<PlayerData> m_playerData = new List<PlayerData>();
	public bool m_turnInProgress;

	public bool ReceivedAllInput()
	{
		for(int i = 0; i < m_playerData.Count; ++i)
		{
			var data = m_playerData[i];
			if (!data.m_receivedAttackInput || !data.m_receivedMoveInput || !data.m_receivedEquipInput)
				return false;
		}
		return true;
	}
}
