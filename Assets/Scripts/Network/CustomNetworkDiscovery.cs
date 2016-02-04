using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class CustomNetworkDiscovery : NetworkDiscovery
{
	private Action<string, string> m_onReceivedBroadcast;

	public void SetOnReceiveBroadcastCallback(Action<string, string> onReceivedBroadcast)
	{
		m_onReceivedBroadcast = onReceivedBroadcast;
	}

	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		base.OnReceivedBroadcast(fromAddress, data);

		if (m_onReceivedBroadcast != null)
			m_onReceivedBroadcast(fromAddress, data);
	}
}
