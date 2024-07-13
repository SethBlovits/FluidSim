using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Logic : MonoBehaviour
{
    
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.AddComponent<Rigidbody>();
        this.GetComponent<Rigidbody>().velocity = 30 * cam.transform.forward;
    }

    void OnTriggerEnter(Collider collision){
        if(collision.gameObject.name == "Character (1)"){
            collision.gameObject.GetComponent<Death_Effect>().dead = true;
        }
    }
    
}
