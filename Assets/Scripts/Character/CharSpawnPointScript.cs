using UnityEngine;
using System.Collections;

public interface IHasRouteScript
{
    void SetPatrolRoute(string r);
}

public class CharSpawnPointScript : MonoBehaviour {

    public GameObject _prefab;
    public string _pathToPatrolRoute;
    public bool _drawGizmos = true;

	void Awake () {
        GameObject obj = Instantiate(_prefab, transform.position, Quaternion.identity) as GameObject;
        IHasRouteScript i = (IHasRouteScript)obj.GetComponent(typeof(IHasRouteScript));
        if (i != null)
            i.SetPatrolRoute("Nav_Overlay/" + _pathToPatrolRoute);
	}

    void OnDrawGizmos()
    {
        if (!_drawGizmos)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, new Vector3(.5f, .5f, .02f));
    }
}
