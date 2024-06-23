using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Script : MonoBehaviour
{
    public Rigidbody m_rigidbody;
    public float speed; 
    public Animator m_animator;
    bool Running = false;
    bool onFloor = false;
    void buttonWatcher(){
        float moveSpeed = 15;
        Vector3 moveVector = Vector3.zero;
        Running = false;
        m_animator.SetBool("Running", false);
        m_animator.SetBool("Walking",false);
        if(Input.GetKey("left shift")){
            moveSpeed = 30;
            Running = true;
            m_animator.SetBool("Running",true);
        }  
        if(Input.GetKey("a")){
        
            moveVector += -transform.right*Time.fixedDeltaTime*moveSpeed;
        }
        if(Input.GetKey("d")){

            moveVector += transform.right*Time.fixedDeltaTime*moveSpeed;
    
        }
        if(Input.GetKey("w")){
        
            moveVector += transform.forward*Time.fixedDeltaTime*moveSpeed;
            
        }
        if(Input.GetKey("s")){

            moveVector += -transform.forward*Time.fixedDeltaTime*moveSpeed;
        }
        if(moveVector!=Vector3.zero){
            if(!Running){
                m_animator.SetBool("Walking",true);
            }
            if(onFloor){
                m_rigidbody.velocity = moveVector*speed;
            }
            else{
                m_rigidbody.velocity += moveVector*.1f;
            }
            //m_rigidbody.velocity = moveVector*speed;
        }
        
        
    }
    void OnCollisionStay(Collision collision)
    {
        //Check to see if the Collider's name is "Chest"
        if (collision.collider.name.Contains("Terrain"))
        {
            //Output the message
            onFloor = true;
        }
    }
    void OnCollisionExit(Collision collision) {
        if (collision.collider.name.Contains("Terrain"))
        {
            //Output the message
            onFloor = false;
        }
    }
    // Update is called once per frame
    void FixedUpdate() 
    {
        buttonWatcher();
        
    }
}

