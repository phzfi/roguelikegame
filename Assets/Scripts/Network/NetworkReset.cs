using UnityEngine;
using System.Collections;

public class NetworkReset : MonoBehaviour {
	public GameObject m_lobbyPrefab;
	public bool m_running = false;
	private bool m_dedicated = false;
	void Start()
	{
		DontDestroyOnLoad(this);
	}

	void Update()
	{
		if(m_running)
		{
			var lobbies = FindObjectsOfType<LobbyManager>();
			for(int i = 0; i < lobbies.Length; ++i)
				lobbies[i].transform.localScale = new Vector3(0, 0, 0);
		}
	}

	public void RestartNetwork() // Destroys old lobby manager and makes a new one to force game into starting state and prevent duplicates
	{
		StartCoroutine(Run());
	}

	private IEnumerator Run() // Restart server with magical delays so that the scene has time to load but the server doesn't have time to restart
	{
		m_running = true;
		float startTime = Time.realtimeSinceStartup;

		while (Time.realtimeSinceStartup - startTime < 1.0f)
			yield return null;

		var lobby = FindObjectOfType<LobbyManager>();
		Destroy(lobby.gameObject);
		Debug.Log("Reset network");

		startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < 2.0f)
			yield return null;

		m_running = false;
		var newlobby = FindObjectOfType<LobbyManager>();
		if (newlobby != null)
		{
			newlobby.transform.localScale = new Vector3(1, 1, 1);
			Debug.Log("Network restarted");
		}
		else
		{
			newlobby = Instantiate(m_lobbyPrefab).GetComponent<LobbyManager>();
			Debug.Log("Network recreated");
		}

		var resetters = FindObjectsOfType<NetworkReset>(); // Destroy additional resetters so they don't clutter the scene
		for (int i = 1; i < resetters.Length; ++i)
			Destroy(resetters[i].gameObject);
	}
}
