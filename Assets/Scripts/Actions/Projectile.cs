using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	private float m_startTime;

	public void FireProjectile(Vector3 start, Vector3 end, float velocity)
	{
		m_startTime = Time.realtimeSinceStartup;
		StartCoroutine(ProjectileCoRoutine(start, end, velocity));
	}

	public IEnumerator ProjectileCoRoutine(Vector3 startPoint, Vector3 endPoint, float velocity)
	{
		Vector3 dir = (endPoint - startPoint);
		transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.forward);

		float distanceToTarget = dir.magnitude;
		Vector3 vel = dir.normalized * velocity;

		while (true) // Repeat until projectile has travelled far enough
		{
			float t = Time.realtimeSinceStartup - m_startTime;

			Vector3 pos = startPoint + vel * t;
			float distanceTravelled = velocity * t;
			if (distanceTravelled > distanceToTarget) // Stop visualization if we have travelled enough
			{
				ClientTurnLogicManager.MarkActionFinished();
				Destroy(gameObject);
				yield break;
			}
			transform.position = pos;
			yield return null;
		}
	}
}
