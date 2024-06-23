using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fishingHookStick : MonoBehaviour
{  
    bool stick = false;
    void Update(){
        if(stick){
            GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        }
        
    }
    private void OnCollisionEnter(Collision other) {
        GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        GetComponent<Rigidbody>().useGravity = false;
        stick = true;
    }
}
