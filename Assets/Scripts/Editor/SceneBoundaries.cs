using UnityEngine;
using UnityEditor;
using System.Collections;

public class SceneBoundaries {

	public void CreateBoundaries(Vector3 sizeOfSetting, GameObject boundaryPrefab, GameObject p) {
		var size = sizeOfSetting * 2.0f;
		size.x -= 1.0f;
		size.y -= 1.0f;
		GameObject obj1 = Object.Instantiate(boundaryPrefab, new Vector3(.0f, sizeOfSetting[1] + .5f, .0f), Quaternion.identity) as GameObject;
		obj1.transform.localScale = new Vector3(size[0], 1.0f, sizeOfSetting[2]);
		obj1.transform.parent = p.transform;

        GameObject obj2 = Object.Instantiate(boundaryPrefab, new Vector3(.0f, -sizeOfSetting[1] - .5f, .0f), Quaternion.identity) as GameObject;
		obj2.transform.localScale = new Vector3(size[0], 1.0f, sizeOfSetting[2]);
		obj2.transform.parent = p.transform;

        GameObject obj3 = Object.Instantiate(boundaryPrefab, new Vector3(sizeOfSetting[0] + .5f, .0f, .0f), Quaternion.identity) as GameObject;
		obj3.transform.localScale = new Vector3(1.0f, size[1],sizeOfSetting[2]);
		obj3.transform.parent = p.transform;

        GameObject obj4 = Object.Instantiate(boundaryPrefab, new Vector3(-sizeOfSetting[0] - .5f, .0f, .0f), Quaternion.identity) as GameObject;
		obj4.transform.localScale = new Vector3(1.0f, size[1], sizeOfSetting[2]);
		obj4.transform.parent = p.transform;

        GameObject obj5 = Object.Instantiate(boundaryPrefab, new Vector3(sizeOfSetting[0] + .5f, sizeOfSetting[1] + .5f, -sizeOfSetting[2] * .5f), Quaternion.identity) as GameObject;
		obj5.transform.localScale = new Vector3(2.0f, 2.0f, sizeOfSetting[2]*2.0f);
		obj5.transform.parent = p.transform;

        GameObject obj6 = Object.Instantiate(boundaryPrefab, new Vector3(sizeOfSetting[0] + .5f, -sizeOfSetting[1] - .5f, -sizeOfSetting[2] * .5f), Quaternion.identity) as GameObject;
		obj6.transform.localScale = new Vector3(2.0f, 2.0f, sizeOfSetting[2]*2.0f);
		obj6.transform.parent = p.transform;

        GameObject obj7 = Object.Instantiate(boundaryPrefab, new Vector3(-sizeOfSetting[0] - .5f, sizeOfSetting[1] + .5f, -sizeOfSetting[2] * .5f), Quaternion.identity) as GameObject;
		obj7.transform.localScale = new Vector3(2.0f, 2.0f, sizeOfSetting[2]*2.0f);
		obj7.transform.parent = p.transform;

        GameObject obj8 = Object.Instantiate(boundaryPrefab, new Vector3(-sizeOfSetting[0] - .5f, -sizeOfSetting[1] - .5f, -sizeOfSetting[2] * .5f), Quaternion.identity) as GameObject;
		obj8.transform.localScale = new Vector3(2.0f, 2.0f, sizeOfSetting[2]*2.0f);
		obj8.transform.parent = p.transform;

		GameObject obj9 = Object.Instantiate(boundaryPrefab, new Vector3(.0f, .0f, .55f*sizeOfSetting[2]), Quaternion.identity) as GameObject;
		obj9.transform.localScale = new Vector3(size[0]+2.5f, size[1]+2.5f, .1f*sizeOfSetting[2]);
		obj9.transform.parent = p.transform;
        obj9.layer = LayerMask.NameToLayer("EnviromentAccessible");
	}
}
