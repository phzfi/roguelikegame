using UnityEngine;
using System.Collections;

public class EnviromentOverlayScript : MonoBehaviour {

    static private int m_envAcc;
    static private int m_envUnAcc;

    public static bool LayerIsEnviroment(int layer)
    {
        m_envAcc = LayerMask.NameToLayer("EnviromentAccessible");
        m_envUnAcc = LayerMask.NameToLayer("EnviromentUnaccessible");
        if(layer == m_envAcc || layer == m_envUnAcc)
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
