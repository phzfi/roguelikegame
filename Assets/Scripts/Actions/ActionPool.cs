using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ActionPool : NetworkBehaviour {
	private uint m_firstFreeIndex = 0;

	[SyncVar]
	private uint m_objectID = 0;
	private static uint sm_lastID = 0;
	[SyncVar]
	private bool initialized = false;

	void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!initialized)
		{
			m_objectID = sm_lastID++;
			initialized = true;
		}
	}
	
	public ulong GetNextFreeID() // pack object ID and action ID into a single unique ID for an action.
	{
		return (((ulong)(m_objectID) << 32) | (m_firstFreeIndex++));
	}

	public static void ResetCounter()
	{
		sm_lastID = 0;
	}
}
