using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindControl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Bone;
    public GameObject Bone2;
    public GameObject Bone3;
    public LODGroup lod;
    public float windIntensity;
    float randomScalarX;
    float randomScalarZ;

    // Update is called once per frame
    void Start(){
        randomScalarX = Random.Range(0f,5f);
        randomScalarZ = Random.Range(0f,5f);
        lod.enabled = true;

    }
    void Update()
    {
        Bone.transform.rotation = Quaternion.Euler(Mathf.Sin(Time.time+randomScalarX)*windIntensity,
        transform.rotation.eulerAngles.y,
        Mathf.Sin(Time.time+randomScalarZ)*windIntensity);

        Bone2.transform.rotation = Quaternion.Euler(Mathf.Sin(Time.time+randomScalarX)*windIntensity,
        transform.rotation.eulerAngles.y,
        Mathf.Sin(Time.time+randomScalarZ)*windIntensity);

        Bone3.transform.rotation = Quaternion.Euler(Mathf.Sin(Time.time+randomScalarX)*windIntensity,
        transform.rotation.eulerAngles.y,
        Mathf.Sin(Time.time+randomScalarZ)*windIntensity);       
    }    
    
}
