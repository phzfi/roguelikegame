using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
	[SerializeField]
	private SyncManager m_syncManager;
    [SerializeField]
    private HostSyncManager m_hostSyncManager;
    [SerializeField]
    private ClientSyncManager m_clientSyncManager;

    public void Start()
	{
        if(m_syncManager)
		    m_syncManager.enabled = false;
        if (m_hostSyncManager)
            m_hostSyncManager.enabled = false;
        if (m_clientSyncManager)
            m_clientSyncManager.enabled = false;
    }

	public override void OnClientConnect(NetworkConnection conn) // called when connected to a server
	{
		base.OnClientConnect(conn);
        if (m_clientSyncManager)
        {
            m_clientSyncManager.enabled = true;
            m_clientSyncManager.InitOnClient(conn);
        }
        else if (m_syncManager)
        {
            m_syncManager.enabled = true;
            m_syncManager.InitOnClient(conn);
        }
        else
            Debug.LogError("Unable to to initialize either syncs");
    }

	public override void OnStartServer() // called when starting server or host
	{
		base.OnStartServer();
        if (m_hostSyncManager)
        {
            m_hostSyncManager.enabled = true;
            m_hostSyncManager.InitOnServer();
        }
        else if (m_syncManager)
        {
            m_syncManager.enabled = true;
            m_syncManager.InitOnServer();
        }
        else
            Debug.LogError("Unable to to initialize either syncs");
	}

	public override void OnServerDisconnect(NetworkConnection conn) // called on server (or host) when client disconnects
	{
		base.OnServerDisconnect(conn);
		m_syncManager.DisconnectClient(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn) // called on client when disconnected from a server
	{
		base.OnClientDisconnect(conn);
		m_syncManager.StopOnClient();
	}

	public override void OnStopServer() // called on server when server (or host) is stopped
	{
		base.OnStopServer();
        if (m_hostSyncManager)
        {
            m_hostSyncManager.StopOnServer();
            m_hostSyncManager.enabled = false;
        }
        else if (m_syncManager)
        {
            m_syncManager.StopOnServer();
            m_syncManager.enabled = false;
        }
    }
}
