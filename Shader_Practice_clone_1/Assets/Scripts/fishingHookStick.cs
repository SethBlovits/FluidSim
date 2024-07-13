using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class fishingHookStick : MonoBehaviour
{  
    bool stick = false;
    //public GameObject tempFish;
    //public GameObject arm;
    public bool inWater;
    public GameObject otherPlayer;

   
    void Update(){
        
        if(stick && otherPlayer == null){
            GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        }
        else if(otherPlayer){
            GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
            transform.position = otherPlayer.transform.position + Vector3.up;
        }
        
    }
    /*
    private void checkForCollision(){
        RaycastHit hit;
        if(Physics.SphereCast(transform.position,1f,Vector3.zero,out hit)){
            if(hit.collider.name == "Pond Water"){
                inWater = true;
            }
            else{
                stick = true;
                GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
                GetComponent<Rigidbody>().useGravity = false;
            }
        }
    }*/
    //if IsServer deal damage else IsClient || IsHost do something else
    /*
    [ServerRpc]
    private void checkForCollisionServerRpc(bool inWater){
        RaycastHit hit;
        if(Physics.SphereCast(transform.position,1f,Vector3.zero,out hit)){
            if(hit.collider.name == "Pond Water"){
                inWater = true;
            }
        }
    }*/
    
    private void OnTriggerEnter(Collider other) {
        
        if(other.name == "Pond Water"){
            inWater = true;
        }
        
        
    }
    private void OnCollisionEnter(Collision other) {
        
        GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        GetComponent<Rigidbody>().useGravity = false;
        stick = true;
        if(other.gameObject.name == "Character(Clone)"){
            otherPlayer = other.gameObject;
            GetComponent<Collider>().enabled = false;
        }
    }
}
