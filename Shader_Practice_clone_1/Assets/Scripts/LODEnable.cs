using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODEnable : MonoBehaviour
{
    // Start is called before the first frame update
    public LODGroup lod;
    void Start()
    {
        lod.enabled = true;
    }
}
