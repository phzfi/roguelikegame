using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {
    [SerializeField]
    private SyncManager m_syncManager;

    public void Start()
    {
        m_syncManager.enabled = false;
    }

    // called when connected to a server
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        m_syncManager.InitOnClient(conn);
    }

    // called when starting server or host
    public override void OnStartServer()
    {
        base.OnStartServer();
        m_syncManager.InitOnServer();
        m_syncManager.enabled = true;
    }

    // called on server (or host) when client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        m_syncManager.DisconnectClient(conn);
    }

    // called on client when disconnected from a server
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        m_syncManager.StopOnClient();
    }

    // called on server when server (or host) is stopped
    public override void OnStopServer()
    {
        base.OnStopServer();
        m_syncManager.StopOnServer();
    }
}
