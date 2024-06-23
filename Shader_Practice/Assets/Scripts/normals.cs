using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class normals : MonoBehaviour
{
    // Start is called before the first frame update
    public Mesh mesh;
    

    // Update is called once per frame
    void Update()
    {
        mesh.RecalculateNormals();    
    }
}
