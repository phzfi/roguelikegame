using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

// Class in charge of running the game logic over network connections. Note that when hosting, the same instance of SyncManager handles both client and server side
public class SyncManager : NetworkBehaviour
{
	public static List<PlayerSync> sm_players = new List<PlayerSync>();
	private static List<MoveOrder> sm_moveOrders = new List<MoveOrder>();
	private static List<MoveOrder> sm_visualizeMoveOrders = new List<MoveOrder>();
	private static List<PickupOrder> sm_pickupOrders = new List<PickupOrder>();
	private static List<AttackOrder> sm_attackOrders = new List<AttackOrder>();
	public float m_syncRate = .5f;
	public float m_timeOutTurn = 1.0f;
	public static int sm_currentTurn = 0;

	private float m_lastSync = -99.0f;

	private static ClientData sm_clientData; // stores all data relevant only to client, null if server
	private static ServerData sm_serverData; // stores all data relevant only to server, null if client
	private static bool sm_isServer = false; // bool that tells if we're either dedicated server or host

	public static bool IsServer
	{
		get { return sm_isServer; }
	}

	void Start()
	{

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
		NetworkServer.RegisterHandler((short)msgType.visualize, OnServerReceiveVisualizationOrders);
		NetworkServer.RegisterHandler((short)msgType.pickupOrder, OnServerReceivePickupOrders);
		NetworkServer.RegisterHandler((short)msgType.attackOrder, OnServerReceiveAttackOrders);
		sm_serverData = new ServerData();
		enabled = true;
		sm_isServer = true;
	}

	public void InitOnClient(NetworkConnection connection) // initialize client side sync logic
	{
		sm_clientData = new ClientData();
		sm_clientData.m_connection = connection;
		sm_clientData.m_clientID = netId.Value;
		sm_clientData.m_connection.RegisterHandler((short)msgType.moveOrder, OnClientReceiveMoveOrders);
		sm_clientData.m_connection.RegisterHandler((short)msgType.connected, OnClientReceiveConnection);
		sm_clientData.m_connection.RegisterHandler((short)msgType.visualize, OnClientReceiveVisualizationOrders);
		sm_clientData.m_connection.RegisterHandler((short)msgType.pickupOrder, OnClientReceivePickupOrders);
		sm_clientData.m_connection.RegisterHandler((short)msgType.attackOrder, OnClientReceiveAttackOrders);
		enabled = true;

		var msg = new ConnectionMessage();
		msg.m_clientID = netId.Value;
		sm_clientData.m_connection.Send((short)msgType.connected, msg); // send netId of this object to server so that it can keep track of connections using it
	}

	public void StopOnServer() // stop server side sync logic
	{
		NetworkServer.UnregisterHandler((short)msgType.moveOrder);
		NetworkServer.UnregisterHandler((short)msgType.connected);
		NetworkServer.UnregisterHandler((short)msgType.visualize);
		NetworkServer.UnregisterHandler((short)msgType.pickupOrder);
		NetworkServer.UnregisterHandler((short)msgType.attackOrder);
		sm_serverData = null;
		enabled = false;
		sm_isServer = false;
	}

	public void StopOnClient() // stop client side sync logic
	{
		sm_clientData.m_connection.UnregisterHandler((short)msgType.moveOrder);
		sm_clientData.m_connection.UnregisterHandler((short)msgType.connected);
		sm_clientData.m_connection.UnregisterHandler((short)msgType.visualize);
		sm_clientData.m_connection.UnregisterHandler((short)msgType.pickupOrder);
		sm_clientData.m_connection.UnregisterHandler((short)msgType.attackOrder);
		sm_clientData = null;
		enabled = false;
	}

	public void DisconnectClient(NetworkConnection connection) // remove disconnected client from players list so that we won't wait for them during turn changes
	{
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

	void handleVisualizeMoveOrdersOnClient()
	{
		for (int i = 0; i < sm_visualizeMoveOrders.Count; ++i)
		{
			var order = sm_visualizeMoveOrders[i];
			MovementManager.OrderMoveVisualize(order);
		}
		sm_visualizeMoveOrders.Clear();
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
			//TODO: do things
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

	void StartServerTurn() // advances the game state by one turn, runs all the server-side game logic
	{
		sm_serverData.m_turnInProgress = true;
		for (int i = 0; i < sm_serverData.m_playerData.Count; ++i)
			sm_serverData.m_playerData[i].m_receivedInput = false;

		RpcRunClientTurn();
		StartCoroutine(ServerTurnCoRoutine());
	}

	IEnumerator ServerTurnCoRoutine() // coroutine that waits until all clients have sent their input for this turn, then finishes the server side turn
	{
		while (true)
		{
			bool receivedAllInput = true;
			for (int i = 0; i < sm_serverData.m_playerData.Count; ++i)
			{
				var playerData = sm_serverData.m_playerData[i];
				if (!playerData.m_receivedInput) // TODO: need to check other input types as well here
				{
					receivedAllInput = false;
					break;
				}
			}
			if (receivedAllInput)
			{
				EndServerTurn();
				yield break;
			}
			if (Time.realtimeSinceStartup - m_lastSync > m_timeOutTurn)
			{
				Debug.Log("Not all clients responded in time, ending turn prematurely");
				EndServerTurn();
				yield break;
			}

			yield return null;
		}
	}

	void EndServerTurn() // finishes the server side turn
	{
		handleMoveOrdersOnServer();
		handleAttackOrdersOnServer();
		MovementManager.RunServerTurn();
		SendVisualizeOrdersToClients();
		SendPickupOrdersToClients();

		sm_currentTurn++;
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

		sm_currentTurn++;
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

	PlayerData GetPlayerDataFromID(uint ID) // gets the player data container associated with given ID
	{
		for (int i = 0; i < sm_serverData.m_playerData.Count; ++i)
		{
			var playerData = sm_serverData.m_playerData[i];
			if (playerData.m_clientID == ID)
				return playerData;
		}
		return null;
	}

	void OnServerReceiveMoveOrders(NetworkMessage msg) // handles move order data received from client
	{
		var moveOrderMessage = msg.ReadMessage<MoveOrderMessage>();
		sm_moveOrders.AddRange(moveOrderMessage.m_orders);

		var playerData = GetPlayerDataFromID(moveOrderMessage.m_clientID);
		if (playerData != null)
			playerData.m_receivedInput = true;
		else
			Debug.Log("Can't find player data for client ID: " + moveOrderMessage.m_clientID);
	}

	void OnClientReceiveMoveOrders(NetworkMessage msg) // placeholder
	{
	}

	void OnServerReceiveVisualizationOrders(NetworkMessage msg) // placeholder
	{
	}

	void OnClientReceiveVisualizationOrders(NetworkMessage msg) // handles received move visualization order data on client
	{
		var visualizeMoveOrderMessage = msg.ReadMessage<MoveOrderMessage>();
		sm_visualizeMoveOrders.AddRange(visualizeMoveOrderMessage.m_orders);
		handleVisualizeMoveOrdersOnClient();

		sm_clientData.m_receivedInput = true;
		sm_clientData.m_turnInProgress = false;
	}

	void OnServerReceiveConnection(NetworkMessage msg) // creates new server data entry for connected client so that they can be tracked when changing turns
	{
		var connectMsg = msg.ReadMessage<ConnectionMessage>(); // when receiving connection message from client, generate server data for the client
		uint clientID = connectMsg.m_clientID;

		var playerData = new PlayerData();
		playerData.m_clientID = clientID;
		playerData.m_connectionID = msg.conn.connectionId;
		playerData.m_connection = msg.conn;
		sm_serverData.m_playerData.Add(playerData);
	}

	void OnClientReceiveConnection(NetworkMessage msg) //placeholder
	{
	}

	void OnServerReceivePickupOrders(NetworkMessage msg) //placeholder
	{
	}

	void OnServerReceiveAttackOrders(NetworkMessage msg)
	{
		var attackMsg = msg.ReadMessage<AttackOrderMessage>();
		sm_attackOrders.AddRange(attackMsg.m_orders);
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

	public static void AddMoveOrder(Vector2i targetGridPos, int moverID)
	{
		if (sm_clientData.m_turnInProgress) // block input when turn processing is in progress. TODO: visualize this somehow
			return;

		var order = new MoveOrder(targetGridPos, moverID);
		sm_moveOrders.Add(order);
	}

	public static void AddAttackOrder(int targetID, int moverID)
	{
		if (sm_clientData.m_turnInProgress) // block input when turn processing is in progress. TODO: visualize this somehow
			return;

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

	void Reset()
	{
		sm_currentTurn = 0;
	}
}
