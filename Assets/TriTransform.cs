using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TriTransform : MonoBehaviour
{
    [SerializeField] private Transform parentContainer;
    [SerializeField] private MeshFilter meshFilter;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        meshFilter = GetComponent<MeshFilter>();
        
        Vector3[] vertices = new Vector3[3];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 100, 0);
        vertices[2] = new Vector3(100, 0, 0);
        
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null){
            meshFilter.mesh = new Mesh();
            mesh = meshFilter.sharedMesh;
        }

        mesh.Clear();
        
        mesh.vertices = vertices;

        mesh.triangles = new int[] { 0, 1, 2 };
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        
    }
}
