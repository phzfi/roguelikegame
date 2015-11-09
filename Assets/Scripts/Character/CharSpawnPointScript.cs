using UnityEngine;
using System.Collections;

public interface IHasRouteScript
{
    void SetPatrolRoute(string r);
}

public class CharSpawnPointScript : MonoBehaviour {

    public GameObject m_prefab;
    public string m_pathToPatrolRoute;
    public bool m_drawGizmos = true;

	void Awake () {
        GameObject obj = Instantiate(m_prefab, transform.position, Quaternion.identity) as GameObject;
        IHasRouteScript i = (IHasRouteScript)obj.GetComponent(typeof(IHasRouteScript));
        if (i != null)
            i.SetPatrolRoute("Nav_Overlay/" + m_pathToPatrolRoute);
	}

    void OnDrawGizmos()
    {
        if (!m_drawGizmos)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, new Vector3(.5f, .5f, .02f));
    }
}
