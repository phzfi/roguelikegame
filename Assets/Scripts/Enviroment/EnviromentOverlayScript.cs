using UnityEngine;
using System.Collections;

public class EnviromentOverlayScript : MonoBehaviour {

    static private int _envAcc;
    static private int _envUnAcc;

    public static bool LayerIsEnviroment(int layer)
    {
        _envAcc = LayerMask.NameToLayer("EnviromentAccessible");
        _envUnAcc = LayerMask.NameToLayer("EnviromentUnaccessible");
        if(layer == _envAcc || layer == _envUnAcc)
            return true;
        else
            return false;
    }

    void Start()
    {
        transform.gameObject.layer = LayerMask.NameToLayer("EnviromentAccessible");
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        transform.gameObject.AddComponent<MeshFilter>();
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
            i++;
        }
        Mesh m = transform.GetComponent<MeshFilter>().mesh;
        m.CombineMeshes(combine);
        transform.gameObject.AddComponent<MeshRenderer>();
        transform.gameObject.GetComponent<MeshRenderer>().sharedMaterial = meshFilters[1].gameObject.GetComponent<MeshRenderer>().sharedMaterial;

        Color[] colors = new Color[m.vertexCount];
        for (int j = 0; j < transform.GetComponent<MeshFilter>().mesh.vertexCount; ++j)
            colors[j] = Color.magenta;
        m.colors = colors;
    }
}
