using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;

// Class in charge of running the game logic over network connections. Note that when hosting, the same instance of SyncManager handles both client and server side
public class SyncManager : NetworkBehaviour
{
    public static List<string> sm_chatLog = new List<string>();

	public static float m_lastSync = -99.0f;

	private static ClientData sm_clientData; // stores all data relevant only to client, null if server
	private static ServerData sm_serverData; // stores all data relevant only to server, null if client
	private static bool sm_isServer = false; // bool that tells if we're either dedicated server or host
	private static bool sm_isClient = false; // are we running a game client locally

	private static List<MoveOrder> sm_moveOrders = new List<MoveOrder>();
	private static List<MoveOrder> sm_visualizeMoveOrders = new List<MoveOrder>();
	private static List<PickupOrder> sm_pickupOrders = new List<PickupOrder>();
	private static List<AttackOrder> sm_attackOrders = new List<AttackOrder>();
    private static List<EquipOrder> sm_equipOrders = new List<EquipOrder>();
	private static List<ActionData> sm_outgoingActions = new List<ActionData>();
	private static List<ActionData> sm_incomingActions = new List<ActionData>();
	private static List<ActionData> sm_outgoingVisualizeActions = new List<ActionData>();
	private static List<ActionData> sm_incomingVisualizeActions = new List<ActionData>();
	
    private ChatManager m_chatManager;
	private ClientTurnLogicManager m_turnLogicManager;
	
	public float m_syncRate = .5f;
	public float m_timeOutTurn = 1.0f;

	public AudioSource m_audioSource;
	public AudioClip m_succesfullInput, m_unsuccesfullInput;

	public static AudioSource sm_audioSource;
	public static AudioClip sm_succesfullInput, sm_unsuccesfullInput;

	public static int sm_currentTurn = 0;
	public static bool sm_running = false;
	private static int sm_clientCount = 0;
    private static bool sm_isVictory = true;

	public static bool IsServer
	{
		get { return sm_isServer; }
	}

	void Awake() // Set editor references to static ones. A bit dumb, maybe there's a better way.
	{
		sm_audioSource = m_audioSource;
		sm_succesfullInput = m_succesfullInput;
		sm_unsuccesfullInput = m_unsuccesfullInput;
		m_chatManager = FindObjectOfType<ChatManager>();
		m_turnLogicManager = FindObjectOfType<ClientTurnLogicManager>();
	}

	public override void OnStartServer()
	{
		Debug.Log("OnStartServer");
		base.OnStartServer();
		InitOnServer();
	}

	public override void OnStartClient()
	{
		Debug.Log("OnStartClient");
		base.OnStartClient();
		InitOnClient(NetworkManager.singleton.client.connection);
	}

	void Update()
	{
		if (sm_isServer && Time.realtimeSinceStartup - m_lastSync > m_syncRate && !sm_serverData.m_turnInProgress) // start turn change if enough time has passed since last turn, and we're on the server
		{
			if (GetClientCount() < sm_clientCount) // If all clients haven't yet sent their connection messages, wait.
				return;
			m_lastSync = Time.realtimeSinceStartup;
			StartServerTurn();
		}
	}

	public static bool GetTurnProgress()
	{
		if (sm_isServer && sm_serverData != null)
			return sm_serverData.m_turnInProgress;
		else if (sm_clientData != null)
			return sm_clientData.m_turnInProgress;
		else
			return true;
	}

	public bool GetTurnProgress(out float progress)
	{
		progress = Mathf.Clamp((Time.realtimeSinceStartup - m_lastSync) / m_syncRate, 0, 1);
		return GetTurnProgress();
    }

	public void InitOnServer() // initialize server side sync logic
	{
		NetworkServer.RegisterHandler((short)msgType.connected, OnServerReceiveConnection);
		NetworkServer.RegisterHandler((short)msgType.pickupOrder, EmptyMessageHandler);
		NetworkServer.RegisterHandler((short)msgType.turnSync, EmptyMessageHandler);
		NetworkServer.RegisterHandler((short)msgType.actionOrder, OnServerRecieveActionOrders);
        NetworkServer.RegisterHandler((short)msgType.equipOrder, OnServerReceiveEquipOrders);
        NetworkServer.RegisterHandler((short)msgType.chatMessage, OnServerReceiveChatMessage);
		NetworkServer.RegisterHandler((short)msgType.visualize, OnServerReceiveVisualizeDone);
        NetworkServer.RegisterHandler((short)msgType.localPlayerDeath, OnServerHandleDeathMessage);
        NetworkServer.RegisterHandler((short)msgType.endMatch, EmptyMessageHandler);
        sm_serverData = new ServerData();
		enabled = true;
		sm_isServer = true;
		sm_running = true;
		gameObject.SetActive(true);
		m_lastSync = -99.0f;
		//NetworkServer.Spawn(gameObject);
	}

    

	public void InitOnClient(NetworkConnection connection) // initialize client side sync logic
	{
		sm_clientData = new ClientData();
		sm_clientData.m_connection = connection;
		sm_clientData.m_clientID = -1;
		sm_clientData.m_connection.RegisterHandler((short)msgType.moveOrder, EmptyMessageHandler);
		sm_clientData.m_connection.RegisterHandler((short)msgType.connected, OnClientReceiveConnection);
		sm_clientData.m_connection.RegisterHandler((short)msgType.visualize, EmptyMessageHandler);
		sm_clientData.m_connection.RegisterHandler((short)msgType.pickupOrder, OnClientReceivePickupOrders);
		sm_clientData.m_connection.RegisterHandler((short)msgType.turnSync, OnClientReceiveTurnSync);
		sm_clientData.m_connection.RegisterHandler((short)msgType.actionOrder, OnClientReceiveActionOrders);
        sm_clientData.m_connection.RegisterHandler((short)msgType.equipOrder, OnClientReceiveEquipOrders);
        sm_clientData.m_connection.RegisterHandler((short)msgType.chatMessage, OnClientReceiveChatMessage);
        sm_clientData.m_connection.RegisterHandler((short)msgType.localPlayerDeath, EmptyMessageHandler);
        sm_clientData.m_connection.RegisterHandler((short)msgType.endMatch, OnClientReceiveEndMatch);
        enabled = true;
		sm_running = true;
		gameObject.SetActive(true);

		var msg = new ConnectionMessage();
		msg.m_clientID = -1;
		m_lastSync = -99.0f;
		sm_clientData.m_connection.Send((short)msgType.connected, msg); // send netId of this object to server so that it can keep track of connections using it
	}


	public void StopOnServer() // stop server side sync logic
	{
		NetworkServer.UnregisterHandler((short)msgType.moveOrder);
		NetworkServer.UnregisterHandler((short)msgType.connected);
		NetworkServer.UnregisterHandler((short)msgType.visualize);
		NetworkServer.UnregisterHandler((short)msgType.pickupOrder);
		NetworkServer.UnregisterHandler((short)msgType.attackOrder);
		NetworkServer.UnregisterHandler((short)msgType.turnSync);
        NetworkServer.UnregisterHandler((short)msgType.equipOrder);
        NetworkServer.UnregisterHandler((short)msgType.localPlayerDeath);
        NetworkServer.UnregisterHandler((short)msgType.endMatch);
        sm_serverData = null;
		enabled = false;
		sm_isServer = false;
		sm_clientCount = 0;
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
		sm_clientData.m_connection.UnregisterHandler((short)msgType.turnSync);
        sm_clientData.m_connection.UnregisterHandler((short)msgType.equipOrder);
        sm_clientData.m_connection.UnregisterHandler((short)msgType.localPlayerDeath);
        sm_clientData.m_connection.UnregisterHandler((short)msgType.endMatch);
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
				sm_clientCount--;
				return;
			}
		}
	}

	public static void IncrementClientCount()
	{
		sm_clientCount++;
	}

	public static void DecrementClientCount()
	{
		sm_clientCount--;
	}

	public int GetClientCount()
	{
		return sm_serverData.m_playerData.Count;
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
		{
			equipment.m_equipment.Add(item.gameObject);
			equipment.m_playerStrength += item.m_strength;
			equipment.m_playerVitality += item.m_vitality;
		}
		else if (!order.m_equipType)
		{
			equipment.m_equipment.Remove(item.gameObject);
			equipment.m_playerStrength -= item.m_strength;
			equipment.m_playerVitality -= item.m_vitality;
		}
	}

    void handleEquipOrdersOnServer()
    {
        for (int i = 0; i < sm_equipOrders.Count; ++i)
			RunEquipOrder(sm_equipOrders[i]);
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

	void handleActionOrdersOnClient()
	{
		//List<ActionData> orderedList = sm_incomingActions.OrderBy(o => o.m_target.m_userID).ToList();
		m_turnLogicManager.StartClientTurnLogic(sm_incomingVisualizeActions);
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
			sm_serverData.m_playerData[i].m_receivedActionInput = false;
			sm_serverData.m_playerData[i].m_receivedEquipInput = false;
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
        handleEquipOrdersOnServer();
        MovementManager.RunServerTurn();
		SendEquipOrdersToClients();
		sm_serverData.StartVisualization();
		SendVisualizeOrdersToClients();
		Debug.Log("start visualization");
		StartCoroutine(WaitForClientVisualizationCoRoutine());
	}

	void FinalizeServerTurn()
	{
		Debug.Log("end visualization");
		SendPickupOrdersToClients();

		sm_currentTurn++;
		SyncTurnNumber();
		m_lastSync = Time.realtimeSinceStartup;
		sm_serverData.m_turnInProgress = false;
		m_lastSync = Time.realtimeSinceStartup;
	}

    void OnServerHandleDeathMessage(NetworkMessage netMsg)
    {
        int players = sm_serverData.m_playerData.Count;
        DeathMessage deathMsg = netMsg.ReadMessage<DeathMessage>(); ;
        if (players != 1)
            --players;

        sm_serverData.m_deathCount += deathMsg.decreaseSize;
        if (sm_serverData.m_deathCount >= players)
        {
            var msg = new EmptyMessage();
            NetworkServer.SendToAll((short)msgType.endMatch, msg);
        }
        else
        {
            Debug.Log("Death registerd on host, death until end: " +
                (players - sm_serverData.m_deathCount));
        }
    }

	IEnumerator WaitForClientVisualizationCoRoutine()
	{
		while(true)
		{
			if (sm_serverData.VisualizationDone())
			{
				FinalizeServerTurn();
				yield break;
			}
			yield return null;
		}
	}

	[ClientRpc]
	void RpcRunClientTurn() // advances client state by one turn, sends input to server
	{
		MovementManager.RunClientTurn();

		sm_clientData.m_turnInProgress = true;
		sm_clientData.m_receivedInput = false;

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
		var msg = new ActionMessage();
		msg.m_actions = sm_outgoingVisualizeActions.ToArray();
		sm_outgoingVisualizeActions.Clear();
		NetworkServer.SendToAll((short)msgType.actionOrder, msg);
	}

	void SendPickupOrdersToClients()
	{
		var msg = new PickupOrderMessage();
		msg.m_orders = sm_pickupOrders.ToArray();
		sm_pickupOrders.Clear();
		NetworkServer.SendToAll((short)msgType.pickupOrder, msg);
		if (GetClientCount() < sm_clientCount)
			Debug.LogError("sent message too early sdjf");
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

    private void OnServerReceiveChatMessage(NetworkMessage netMsg)
    {
        NetworkServer.SendToAll((short)msgType.chatMessage, netMsg.ReadMessage<ChatMessage>());
        sm_chatLog.Add(netMsg.ReadMessage<ChatMessage>().m_message);
        //TODO: add to chatlog
    }

	public static void OnClientSendVisualizeDone()
	{
		ConnectionMessage msg = new ConnectionMessage();
		msg.m_clientID = sm_clientData.m_clientID;
		sm_clientData.m_connection.Send((short)msgType.visualize, msg);
		m_lastSync = Time.realtimeSinceStartup;
    }

	private void OnServerReceiveVisualizeDone(NetworkMessage netMsg)
	{
		var msg = netMsg.ReadMessage<ConnectionMessage>();
		for(int i = 0; i < sm_serverData.m_playerData.Count; ++i)
		{
			var data = sm_serverData.m_playerData[i];
			if (data.m_connectionID == msg.m_clientID)
			{
				data.m_visualizationInProgress = false;
			}
		}
	}

    private void OnClientReceiveChatMessage(NetworkMessage netMsg)
    {
        m_chatManager.AddMessage(netMsg.ReadMessage<ChatMessage>().m_message);
    }

	void OnClientReceiveTurnSync(NetworkMessage msg)
	{
		var syncMsg = msg.ReadMessage<TurnSyncMessage>();
		sm_currentTurn = syncMsg.m_turnNumber;
		sm_clientData.m_receivedInput = true;
		sm_clientData.m_turnInProgress = false;
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

		var playerData = GetPlayerDataFromID(actionMessage.m_clientID);
		if (playerData != null)
			playerData.m_receivedActionInput = true;
		else
			Debug.Log("Can't find player data for client ID: " + actionMessage.m_clientID);
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
		sm_incomingVisualizeActions.AddRange(actionMessage.m_actions);
		handleActionOrdersOnClient();
	}

	void OnClientReceiveConnection(NetworkMessage msg)
	{
		var connectMsg = msg.ReadMessage<ConnectionMessage>(); // when receiving connection message from client, generate server data for the client
		int clientID = connectMsg.m_clientID;
		sm_clientData.m_clientID = clientID;
	}

	void OnClientReceivePickupOrders(NetworkMessage msg) // handle received item pickup orders on client
	{
		var pickupMsg = msg.ReadMessage<PickupOrderMessage>();
		sm_pickupOrders.AddRange(pickupMsg.m_orders);
		handlePickupOrdersOnClient();
	}

    void OnClientReceiveEndMatch(NetworkMessage msg) // handle received item pickup orders on client
    {
        StartCoroutine("WaitForEnd");
    }

    public static void AddChatMessage(string message, int id)
    {
        var msg = new ChatMessage();
        msg.m_clientID = id;
        msg.m_message = message;
        sm_clientData.m_connection.Send((short)msgType.chatMessage, msg);
    }

    public static void SendDeathMessage(int size)
    {
        sm_isVictory = false;
        var msg = new DeathMessage();
        msg.decreaseSize = size;
        sm_clientData.m_connection.Send((short)msgType.localPlayerDeath, msg);
    }

	public static bool CheckInputPossible(bool playSounds = true, bool onlyCancelSounds = false)
	{
		if (GetTurnProgress())
		{
			if(playSounds)
				sm_audioSource.PlayOneShot(sm_unsuccesfullInput);
			return false;
		}
		if(playSounds && ! onlyCancelSounds)
			sm_audioSource.PlayOneShot(sm_succesfullInput);
		return true;
	}

    public static void AddEquipOrder(int itemID, int playerID, bool equipType)
    {
        var order = new EquipOrder(equipType, itemID, playerID);
        sm_equipOrders.Add(order);
    }
	
	public static void AddAction(ActionData action, bool ClearPrevious = true) // Version that ignores turn progress
	{
		if (ClearPrevious)
			sm_outgoingActions.Clear();

		sm_outgoingActions.Add(action);
	}

	public static void AddVisualizeAction(ActionData action)
	{
		sm_outgoingVisualizeActions.Add(action);
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

    private IEnumerator WaitForEnd()
    {
        ActionBar actionBar = FindObjectOfType<ActionBar>();
        for (;;)
        {
            if (!sm_clientData.m_turnInProgress)
                break;
            yield return null;
        }
        actionBar.m_exitMenu.SetEndGameText(!sm_isVictory);
        actionBar.ExitGameButtonPressed();
    }
}
