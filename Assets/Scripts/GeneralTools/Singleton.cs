using UnityEngine;

//From http://wiki.unity3d.com/index.php/Singleton

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T m_instance;
	private static object m_lock = new object();
	private static bool sm_applicationIsQuitting = false;

	public static T Instance
	{
		get
		{
			if (sm_applicationIsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			lock (m_lock)
			{
				if (m_instance == null)
				{
					m_instance = (T)FindObjectOfType(typeof(T));

					if (FindObjectsOfType(typeof(T)).Length > 1)
					{
						Debug.LogError("[Singleton] Something went really wrong there should never be more than 1 singleton!" +
							" Reopening the scene might fix it.");
						return m_instance;
					}

					if (m_instance == null)
					{
						Debug.LogError("[Singleton] No instance of '" + typeof(T) + "' singleton was found in the scene!");
						Debug.Break();
					}
				}

				return m_instance;
			}
		}
	}

	public void OnDestroy()
	{
		sm_applicationIsQuitting = true;
	}
}
