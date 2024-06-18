using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonView : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject fishingRod;
    void Start()
    {
        fishingRod.transform.position = transform.position + transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        fishingRod.transform.position = transform.position+transform.forward; 
    }
}
