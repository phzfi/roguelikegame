using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

// Class in charge of running the game logic over network connections. Note that when hosting, the same instance of SyncManager handles both client and server side
public class SyncManager : NetworkBehaviour
{
    public static List<string> sm_chatLog = new List<string>();

	private float m_lastSync = -99.0f;

	private static ClientData sm_clientData; // stores all data relevant only to client, null if server
	private static ServerData sm_serverData; // stores all data relevant only to server, null if client
	private static bool sm_isServer = false; // bool that tells if we're either dedicated server or host

	private static List<MoveOrder> sm_moveOrders = new List<MoveOrder>();
	private static List<MoveOrder> sm_visualizeMoveOrders = new List<MoveOrder>();
	private static List<PickupOrder> sm_pickupOrders = new List<PickupOrder>();
	private static List<AttackOrder> sm_attackOrders = new List<AttackOrder>();
	private static List<DeathOrder> sm_deathOrders = new List<DeathOrder>();
    private static List<EquipOrder> sm_equipOrders = new List<EquipOrder>();
	private static List<ActionData> sm_outgoingActions = new List<ActionData>();
	private static List<ActionData> sm_incomingActions = new List<ActionData>();
	
    private ChatManager m_chatManager;
	
	public float m_syncRate = .5f;
	public float m_timeOutTurn = 1.0f;
	
	public static int sm_currentTurn = 0;
	public static bool sm_running = false;

	public static bool IsServer
	{
		get { return sm_isServer; }
	}


	void Start()
	{
        m_chatManager = FindObjectOfType<ChatManager>();
	}

	void Update()
	{
		if (sm_isServer && Time.realtimeSinceStartup - m_lastSync > m_syncRate && !sm_serverData.m_turnInProgress) // start turn change if enough time has passed since last turn, and we're on the server
		{
			m_lastSync = Time.realtimeSinceStartup;
			StartServerTurn();
		}
	}

	public void InitOnServer() // initialize server side sync logic
	{
		NetworkServer.RegisterHandler((short)msgType.moveOrder, OnServerReceiveMoveOrders);
		NetworkServer.RegisterHandler((short)msgType.connected, OnServerReceiveConnection);
		NetworkServer.RegisterHandler((short)msgType.visualize, EmptyMessageHandler);
		NetworkServer.RegisterHandler((short)msgType.pickupOrder, EmptyMessageHandler);
		NetworkServer.RegisterHandler((short)msgType.attackOrder, OnServerReceiveAttackOrders);
		NetworkServer.RegisterHandler((short)msgType.death, EmptyMessageHandler);
		NetworkServer.RegisterHandler((short)msgType.turnSync, EmptyMessageHandler);
		NetworkServer.RegisterHandler((short)msgType.actionOrder, OnServerRecieveActionOrders);
        NetworkServer.RegisterHandler((short)msgType.equipOrder, OnServerReceiveEquipOrders);
        NetworkServer.RegisterHandler((short)msgType.chatMessage, OnServerReceiveChatMessage);
        sm_serverData = new ServerData();
		enabled = true;
		sm_isServer = true;
		sm_running = true;		
		gameObject.SetActive(true);
		NetworkServer.Spawn(gameObject);
	}

    

	public void InitOnClient(NetworkConnection connection) // initialize client side sync logic
	{
		sm_clientData = new ClientData();
		sm_clientData.m_connection = connection;
		sm_clientData.m_clientID = -1;
		sm_clientData.m_connection.RegisterHandler((short)msgType.moveOrder, EmptyMessageHandler);
		sm_clientData.m_connection.RegisterHandler((short)msgType.connected, OnClientReceiveConnection);
		sm_clientData.m_connection.RegisterHandler((short)msgType.visualize, OnClientReceiveVisualizationOrders);
		sm_clientData.m_connection.RegisterHandler((short)msgType.pickupOrder, OnClientReceivePickupOrders);
		sm_clientData.m_connection.RegisterHandler((short)msgType.attackOrder, OnClientReceiveAttackOrders);
		sm_clientData.m_connection.RegisterHandler((short)msgType.death, OnClientReceiveDeath);
		sm_clientData.m_connection.RegisterHandler((short)msgType.turnSync, OnClientReceiveTurnSync);
		sm_clientData.m_connection.RegisterHandler((short)msgType.actionOrder, OnClientReceiveActionOrders);
        sm_clientData.m_connection.RegisterHandler((short)msgType.equipOrder, OnClientReceiveEquipOrders);
        sm_clientData.m_connection.RegisterHandler((short)msgType.chatMessage, OnClientReceiveChatMessage);
        enabled = true;
		sm_running = true;
		gameObject.SetActive(true);

		var msg = new ConnectionMessage();
		msg.m_clientID = -1;
		sm_clientData.m_connection.Send((short)msgType.connected, msg); // send netId of this object to server so that it can keep track of connections using it
	}


	public void StopOnServer() // stop server side sync logic
	{
		NetworkServer.UnregisterHandler((short)msgType.moveOrder);
		NetworkServer.UnregisterHandler((short)msgType.connected);
		NetworkServer.UnregisterHandler((short)msgType.visualize);
		NetworkServer.UnregisterHandler((short)msgType.pickupOrder);
		NetworkServer.UnregisterHandler((short)msgType.attackOrder);
		NetworkServer.UnregisterHandler((short)msgType.death);
		NetworkServer.UnregisterHandler((short)msgType.turnSync);
        NetworkServer.UnregisterHandler((short)msgType.equipOrder);
        sm_serverData = null;
		enabled = false;
		sm_isServer = false;
	}

	public void StopOnClient() // stop client side sync logic
	{
		if (sm_clientData != null)
		{
			sm_clientData.m_connection.UnregisterHandler((short)msgType.moveOrder);
			sm_clientData.m_connection.UnregisterHandler((short)msgType.connected);
			sm_clientData.m_connection.UnregisterHandler((short)msgType.visualize);
			sm_clientData.m_connection.UnregisterHandler((short)msgType.pickupOrder);
			sm_clientData.m_connection.UnregisterHandler((short)msgType.attackOrder);
			sm_clientData.m_connection.UnregisterHandler((short)msgType.death);
			sm_clientData.m_connection.UnregisterHandler((short)msgType.turnSync);
			sm_clientData.m_connection.UnregisterHandler((short)msgType.equipOrder);
		}
        sm_clientData = null;
		enabled = false;
	}

	public void DisconnectClient(NetworkConnection connection) // remove disconnected client from players list so that we won't wait for them during turn changes
	{
		if (sm_serverData == null)
			return;

        for (int i = 0; i < sm_serverData.m_playerData.Count; ++i)
		{
			var playerData = sm_serverData.m_playerData[i];
			if (playerData.m_connectionID == connection.connectionId) // tell server to stop tracking disconnected client
			{
				sm_serverData.m_playerData.RemoveAt(i);
				return;
			}
		}
	}

	void handleMoveOrdersOnServer()
	{
		for (int i = 0; i < sm_moveOrders.Count; ++i)
		{
			var order = sm_moveOrders[i];
			MovementManager.OrderMove(order);
		}
		sm_moveOrders.Clear();
	}

	void RunEquipOrder(EquipOrder order)
	{
		var player = CharManager.GetObject(order.m_playerID);
		var item = ItemManager.GetItem(order.m_itemID);
		if (player == null)
		{
			Debug.Log("player not found for equip order");
			return;
		}
		if (item == null)
		{
			Debug.Log("item not found for equip order");
			return;
		}
		var equipment = player.GetComponent<Equipment>();
		if (equipment == null)
		{
			Debug.Log("equipment not found for equip order");
			return;
		}
		if (order.m_equipType && !equipment.m_equipment.Contains(item.gameObject)) // make sure we haven't already equipped this item (host will have run this in server-side code already)
			equipment.m_equipment.Add(item.gameObject);
		else if (!order.m_equipType)
			equipment.m_equipment.Remove(item.gameObject);
	}

    void handleEquipOrdersOnServer()
    {
        for (int i = 0; i < sm_equipOrders.Count; ++i)
			RunEquipOrder(sm_equipOrders[i]);
    }

    void handleVisualizeMoveOrdersOnClient()
	{
		for (int i = 0; i < sm_visualizeMoveOrders.Count; ++i)
		{
			var order = sm_visualizeMoveOrders[i];
			MovementManager.OrderMoveVisualize(order);
		}
		sm_visualizeMoveOrders.Clear();
	}

    void handleEquipOrdersOnClient()
    {
        for (int i = 0; i < sm_equipOrders.Count; ++i)
			RunEquipOrder(sm_equipOrders[i]);
        sm_equipOrders.Clear();
    }

    void handlePickupOrdersOnClient()
	{
		for (int i = 0; i < sm_pickupOrders.Count; ++i)
		{
			var order = sm_pickupOrders[i];
			ItemManager.OrderPickup(order);
		}
		sm_pickupOrders.Clear();
	}

	void handleAttackOrdersOnClient()
	{
		for (int i = 0; i < sm_attackOrders.Count; ++i)
		{
			//var order = sm_attackOrders[i];
			//TODO: visualize attacks
		}
		sm_attackOrders.Clear();
	}

	void handleAttackOrdersOnServer()
	{
		for (int i = 0; i < sm_attackOrders.Count; ++i)
		{
			var order = sm_attackOrders[i];
			MovementManager.OrderAttack(order);
		}
		sm_attackOrders.Clear();
	}

	void handleActionOrdersOnClient()
	{
		for (int i = 0; i < sm_incomingActions.Count; ++i)
		{
			//var order = sm_incomingActions[i];
			//TODO: visualize actions
		}
		sm_incomingActions.Clear();
	}

	void handleActionOrdersOnServer()
	{
		for(int i = 0; i < sm_incomingActions.Count; ++i)
		{
			var actionData = sm_incomingActions[i];
			var action = ActionManager.GetAction(actionData.m_actionID);
			action.Use(actionData.m_target);
		}
		sm_incomingActions.Clear();
	}

	void StartServerTurn() // advances the game state by one turn, runs all the server-side game logic
	{
		sm_serverData.m_turnInProgress = true;
		for (int i = 0; i < sm_serverData.m_playerData.Count; ++i)
		{
			sm_serverData.m_playerData[i].m_receivedMoveInput = false;
			sm_serverData.m_playerData[i].m_receivedAttackInput = false;
		}

		RpcRunClientTurn();
		StartCoroutine(ServerTurnCoRoutine());
	}

	IEnumerator ServerTurnCoRoutine() // coroutine that waits until all clients have sent their input for this turn, then finishes the server side turn
	{
		while (true)
		{
			if (sm_serverData.ReceivedAllInput())
			{
				EndServerTurn();
				yield break;
			}
			if (Time.realtimeSinceStartup - m_lastSync > m_timeOutTurn)
			{
				Debug.Log("Not all clients responded in time, ending turn prematurely");
				sm_serverData.ReceivedAllInput();
                EndServerTurn();
				yield break;
			}

			yield return null;
		}
	}

	void EndServerTurn() // finishes the server side turn
	{
		handleActionOrdersOnServer();
		handleMoveOrdersOnServer();
		handleAttackOrdersOnServer();
        handleEquipOrdersOnServer();
        MovementManager.RunServerTurn();
		SendVisualizeOrdersToClients();
		SendPickupOrdersToClients();
		SendDeathOrdersToClients();
        SendEquipOrdersToClients();

		sm_currentTurn++;
		SyncTurnNumber();
		m_lastSync = Time.realtimeSinceStartup;
		sm_serverData.m_turnInProgress = false;
	}

	[ClientRpc]
	void RpcRunClientTurn() // advances client state by one turn, sends input to server
	{
		MovementManager.RunClientTurn();

		sm_clientData.m_turnInProgress = true;
		sm_clientData.m_receivedInput = false;

		var attackMsg = new AttackOrderMessage();
		AttackOrder[] attackOrders = sm_attackOrders.ToArray();
		sm_attackOrders.Clear();
		attackMsg.m_orders = attackOrders;
		attackMsg.m_clientID = sm_clientData.m_clientID;
		sm_clientData.m_connection.Send((short)msgType.attackOrder, attackMsg);

		var msg = new MoveOrderMessage();
		MoveOrder[] orders = sm_moveOrders.ToArray();
		sm_moveOrders.Clear();
		msg.m_orders = orders;
		msg.m_clientID = sm_clientData.m_clientID;
		sm_clientData.m_connection.Send((short)msgType.moveOrder, msg);

        var equipMsg = new EquipOrderMessage();
        EquipOrder[] equipOrders = sm_equipOrders.ToArray();
        sm_equipOrders.Clear();
		equipMsg.m_orders = equipOrders;
		equipMsg.m_clientID = sm_clientData.m_clientID;
        sm_clientData.m_connection.Send((short)msgType.equipOrder, equipMsg);

		var actionMsg = new ActionMessage();
		ActionData[] actions = sm_outgoingActions.ToArray();
		sm_outgoingActions.Clear();
		actionMsg.m_actions = actions;
		actionMsg.m_clientID = sm_clientData.m_clientID;
		sm_clientData.m_connection.Send((short)msgType.actionOrder, actionMsg);
    }

	void SendVisualizeOrdersToClients()
	{
		var msg = new MoveOrderMessage();
		msg.m_orders = sm_visualizeMoveOrders.ToArray();
		sm_visualizeMoveOrders.Clear();
		NetworkServer.SendToAll((short)msgType.visualize, msg);
	}

	void SendPickupOrdersToClients()
	{
		var msg = new PickupOrderMessage();
		msg.m_orders = sm_pickupOrders.ToArray();
		sm_pickupOrders.Clear();
		NetworkServer.SendToAll((short)msgType.pickupOrder, msg);
	}

	void SyncTurnNumber(NetworkConnection conn = null)
	{
		var msg = new TurnSyncMessage();
		msg.m_turnNumber = sm_currentTurn;
		if (conn == null) // If no connection specified, send to all clients. Otherwise send to just one player.
			NetworkServer.SendToAll((short)msgType.turnSync, msg);
		else
			conn.Send((short)msgType.turnSync, msg);
	}

	void SendDeathOrdersToClients()
	{
		var msg = new DeathMessage();
		msg.m_orders = sm_deathOrders.ToArray();
		sm_deathOrders.Clear();
		NetworkServer.SendToAll((short)msgType.death, msg);
	}

    void SendEquipOrdersToClients()
    {
        var msg = new EquipOrderMessage();
        msg.m_orders = sm_equipOrders.ToArray();
        sm_equipOrders.Clear();
        NetworkServer.SendToAll((short)msgType.equipOrder, msg);
    }

    PlayerData GetPlayerDataFromID(int ID) // gets the player data container associated with given ID
	{
		for (int i = 0; i < sm_serverData.m_playerData.Count; ++i)
		{
			var playerData = sm_serverData.m_playerData[i];
			if (playerData.m_connectionID == ID)
				return playerData;
		}
		return null;
	}
	void EmptyMessageHandler(NetworkMessage msg) // placeholder
	{
	}

	void OnServerReceiveMoveOrders(NetworkMessage msg) // handles move order data received from client
	{
		var moveOrderMessage = msg.ReadMessage<MoveOrderMessage>();

		var playerData = GetPlayerDataFromID(moveOrderMessage.m_clientID);
		if (playerData != null)
		{
			playerData.m_receivedMoveInput = true;
			for (int i = 0; i < moveOrderMessage.m_orders.Length; ++i)
			{
				var order = moveOrderMessage.m_orders[i];
				if (order.IsValid())
					sm_moveOrders.Add(order);
			}
		}
		else
			Debug.Log("Can't find player data for client ID: " + moveOrderMessage.m_clientID);
	}

    private void OnServerReceiveChatMessage(NetworkMessage netMsg)
    {
        NetworkServer.SendToAll((short)msgType.chatMessage, netMsg.ReadMessage<ChatMessage>());
        sm_chatLog.Add(netMsg.ReadMessage<ChatMessage>().m_message);
        //TODO: add to chatlog
    }

    private void OnClientReceiveChatMessage(NetworkMessage netMsg)
    {
        m_chatManager.AddMessage(netMsg.ReadMessage<ChatMessage>().m_message);
    }

	void OnClientReceiveDeath(NetworkMessage msg)
	{
		var deathMsg = msg.ReadMessage<DeathMessage>();
		for (int i = 0; i < deathMsg.m_orders.Length; ++i)
			MovementManager.KillObject(deathMsg.m_orders[i].m_targetID);
	}

	void OnClientReceiveTurnSync(NetworkMessage msg)
	{
		var syncMsg = msg.ReadMessage<TurnSyncMessage>();
		sm_currentTurn = syncMsg.m_turnNumber;
		sm_clientData.m_receivedInput = true;
		sm_clientData.m_turnInProgress = false;
	}

	void OnClientReceiveVisualizationOrders(NetworkMessage msg) // handles received move visualization order data on client
	{
		var visualizeMoveOrderMessage = msg.ReadMessage<MoveOrderMessage>();
		sm_visualizeMoveOrders.AddRange(visualizeMoveOrderMessage.m_orders);
		handleVisualizeMoveOrdersOnClient();
	}

    void OnClientReceiveEquipOrders(NetworkMessage msg)
    {
        var equipOrderMessage = msg.ReadMessage<EquipOrderMessage>();
        sm_equipOrders.AddRange(equipOrderMessage.m_orders);
        handleEquipOrdersOnClient();
    }

	void OnServerReceiveEquipOrders(NetworkMessage msg) //TODO: Add to different lists, coming and outgoing
	{
		var equipOrderMessage = msg.ReadMessage<EquipOrderMessage>();
		var playerData = GetPlayerDataFromID(equipOrderMessage.m_clientID);
		if (playerData != null)
		{
			sm_equipOrders.AddRange(equipOrderMessage.m_orders);
			playerData.m_receivedEquipInput = true;
		}
		else
			Debug.Log("Couldn't find player data for equip order sender!");
    }

	void OnServerRecieveActionOrders(NetworkMessage msg)
	{
		var actionMessage = msg.ReadMessage<ActionMessage>();
		sm_incomingActions.AddRange(actionMessage.m_actions);
	}

    void OnServerReceiveConnection(NetworkMessage msg) // creates new server data entry for connected client so that they can be tracked when changing turns
	{
		var connectMsg = msg.ReadMessage<ConnectionMessage>(); // when receiving connection message from client, generate server data for the client

		var playerData = new PlayerData();
		playerData.m_connectionID = msg.conn.connectionId;
		playerData.m_connection = msg.conn;
		sm_serverData.m_playerData.Add(playerData);

		SyncTurnNumber(playerData.m_connection);

		connectMsg.m_clientID = msg.conn.connectionId;
		msg.conn.Send((short)msgType.connected, connectMsg);
	}

	void OnClientReceiveActionOrders(NetworkMessage msg)
	{
		var actionMessage = msg.ReadMessage<ActionMessage>();
		sm_incomingActions.AddRange(actionMessage.m_actions);
		handleActionOrdersOnClient();
	}

	void OnClientReceiveConnection(NetworkMessage msg)
	{
		var connectMsg = msg.ReadMessage<ConnectionMessage>(); // when receiving connection message from client, generate server data for the client
		int clientID = connectMsg.m_clientID;
		sm_clientData.m_clientID = clientID;
	}
	

	void OnServerReceiveAttackOrders(NetworkMessage msg)
	{
		var attackMsg = msg.ReadMessage<AttackOrderMessage>();

		var playerData = GetPlayerDataFromID(attackMsg.m_clientID);
		if (playerData != null)
		{
			playerData.m_receivedAttackInput = true;
			for(int i = 0; i < attackMsg.m_orders.Length; ++i)
			{
				var order = attackMsg.m_orders[i];
				if(order.IsValid())
					sm_attackOrders.Add(order);
			}
		}
		else
			Debug.Log("Can't find player data for client ID: " + attackMsg.m_clientID);

	}

	void OnClientReceiveAttackOrders(NetworkMessage msg)
	{
		var attackMsg = msg.ReadMessage<AttackOrderMessage>();
		sm_attackOrders.AddRange(attackMsg.m_orders);
		handleAttackOrdersOnClient();
	}

	void OnClientReceivePickupOrders(NetworkMessage msg) // handle received item pickup orders on client
	{
		var pickupMsg = msg.ReadMessage<PickupOrderMessage>();
		sm_pickupOrders.AddRange(pickupMsg.m_orders);
		handlePickupOrdersOnClient();
	}

    public static void AddChatMessage(string message, int id)
    {
        var msg = new ChatMessage();
        msg.m_clientID = id;
        msg.m_message = message;
        sm_clientData.m_connection.Send((short)msgType.chatMessage, msg);
    }

	public static void AddMoveOrder(Vector2i targetGridPos, int moverID)
	{
		if (sm_clientData.m_turnInProgress) // block input when turn processing is in progress. TODO: visualize this somehow
			return;

		var order = new MoveOrder(targetGridPos, moverID);
		sm_moveOrders.Add(order);
	}

	public static void AddDeathOrder(int targetID)
	{
		var order = new DeathOrder(targetID);
		sm_deathOrders.Add(order);
	}

    public static void AddEquipOrder(int itemID, int playerID, bool equipType)
    {
        if (sm_clientData.m_turnInProgress) // block input when turn processing is in progress. TODO: visualize this somehow
            return;

        var order = new EquipOrder(equipType, itemID, playerID);
        sm_equipOrders.Add(order);
    }

    public static void AddAttackOrder(int targetID, int moverID)
	{
		if (sm_clientData.m_turnInProgress) // block input when turn processing is in progress. TODO: visualize this somehow
			return;

		var order = new AttackOrder(moverID, targetID);
		sm_attackOrders.Add(order);
	}

	public static void AddAction(ActionData action)
	{
		if (sm_clientData.m_turnInProgress)
			return;

		sm_outgoingActions.Clear(); // TODO: chained actions
		sm_outgoingActions.Add(action);
	}

	public static void AddServerAttackOrder(int targetID, int moverID) // Version that ignores turn in progress
	{
		var order = new AttackOrder(moverID, targetID);
		sm_attackOrders.Add(order);
	}

	public static void AddMoveVisualizationOrder(Vector2i targetGridPos, int moverID)
	{
		var order = new MoveOrder(targetGridPos, moverID);
		sm_visualizeMoveOrders.Add(order);
	}

	// add new item pickup order to input list
	public static void AddPickupOrder(int moverID, int itemID)
	{
		var order = new PickupOrder(moverID, itemID);
		sm_pickupOrders.Add(order);
	}

	public static bool IsTurnInProgress()
	{
		if (sm_clientData == null)
			return false;
		return sm_clientData.m_turnInProgress;
	}

	void Reset()
	{
		sm_currentTurn = 0;
	}
}
