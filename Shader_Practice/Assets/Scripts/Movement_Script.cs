using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class Movement_Script : NetworkBehaviour
{
    public Rigidbody m_rigidbody;
    public float speed; 
    public Animator m_animator;
    bool Running = false;
    public bool onFloor = false;
    public float jumpHeight;
    public float m_Drag;
    float walkSpeed = 1;
    float runSpeed = 2;
   
    void buttonWatcher(){
        float moveSpeed = walkSpeed;
        Vector3 moveVector = Vector3.zero;
        Running = false;
        m_animator.SetBool("Running", false);
        m_animator.SetBool("Walking",false);
        if(Input.GetKey("left shift")){
            moveSpeed = runSpeed;
            Running = true;
            m_animator.SetBool("Running",true);
        }  
        if(Input.GetKey("a")){
        
            moveVector += -transform.right*moveSpeed;
        }
        if(Input.GetKey("d")){

            moveVector += transform.right*moveSpeed;
    
        }
        if(Input.GetKey("w")){
        
            moveVector += transform.forward*moveSpeed;
            
        }
        if(Input.GetKey("s")){

            moveVector += -transform.forward*moveSpeed;
        }
        if(moveVector!=Vector3.zero){
            if(!Running){
                m_animator.SetBool("Walking",true);
            }
            /*if(onFloor){
                m_rigidbody.velocity = moveVector*speed;
            }*/
            if(!onFloor){
                AirControl(moveVector);
            }
            else{
                m_rigidbody.AddForce(moveVector,ForceMode.VelocityChange);
            }
            
            
            //m_rigidbody.AddForce(moveVector,ForceMode.Impulse);
            //SendMovementServerRpc(moveVector);
            //Debug.Log(m_rigidbody.velocity);
            /*
            else{
                m_rigidbody.velocity += moveVector*.1f;
            }
            */
            //m_rigidbody.velocity = moveVector*speed;
        }
        
        
    }
    void AirControl(Vector3 moveVector){
        Vector3 flatVelocity = new Vector3(m_rigidbody.velocity.x,0,m_rigidbody.velocity.z);
        if(flatVelocity.magnitude>speed*runSpeed){
            //m_rigidbody.AddForce(moveVector,ForceMode.Impulse);
            if(Mathf.Abs(flatVelocity.x + moveVector.x) < Mathf.Abs(flatVelocity.x)){
                m_rigidbody.AddForce(new Vector3(moveVector.x,0,0),ForceMode.VelocityChange);
            }
            if(Mathf.Abs(flatVelocity.z + moveVector.z) < Mathf.Abs(flatVelocity.z)){
                m_rigidbody.AddForce(new Vector3(0,0,moveVector.z),ForceMode.VelocityChange);
            }
        }
        else{
            m_rigidbody.AddForce(moveVector,ForceMode.VelocityChange);
        }
    }
    void groundedDrag(){
        if(onFloor){
            m_rigidbody.drag = m_Drag;
        }
        else{
            m_rigidbody.drag = 0;
        }
    }
    void checkCrouch(){
        
        if(Input.GetKey(KeyCode.LeftControl)){
            m_animator.SetBool("Crouching",true);
            
        }
        else{
            m_animator.SetBool("Crouching",false);
        }
    }
    void speedControl(){
        Vector3 flatVelocity = new Vector3(m_rigidbody.velocity.x,0,m_rigidbody.velocity.z);
        m_animator.SetFloat("Velocity",flatVelocity.magnitude);
        if(onFloor){
            if(Running){
            if(flatVelocity.magnitude>speed*runSpeed){
                flatVelocity = flatVelocity.normalized*speed;
                m_rigidbody.velocity = new Vector3(flatVelocity.x,m_rigidbody.velocity.y,flatVelocity.z);
            }
            }
            else{
                if(flatVelocity.magnitude>speed*walkSpeed){
                    flatVelocity = flatVelocity.normalized*speed;
                    m_rigidbody.velocity = new Vector3(flatVelocity.x,m_rigidbody.velocity.y,flatVelocity.z);
                }
            }
        }
        
    }
    void checkGroundRay(){
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position+transform.up, -transform.up, out hit, 1.2f))
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            //Debug.Log("Did Hit");
            onFloor = true;
        }
        else{
            onFloor = false;
        }
    }
    void Jump(){
        if(Input.GetKey(KeyCode.Space) && onFloor){
            m_animator.SetBool("Jump",true);
            m_rigidbody.velocity = new Vector3(m_rigidbody.velocity.x,0,m_rigidbody.velocity.z);
            m_rigidbody.AddForce(transform.up * jumpHeight,ForceMode.VelocityChange);
            
        }
    }
    void drawFloor(){
        Debug.DrawRay(transform.position+transform.up,-transform.up);
    }
    
    void OnTriggerEnter(Collider collision)
    {
        Debug.Log(collision.name);
        if (collision.name.Contains("Terrain"))
        {
            //Output the message
            onFloor = true;
            m_animator.SetBool("Landed",true);
            m_animator.SetBool("Jump",false);
        }
    }
    void OnTriggerExit(Collider collision) {
        if (collision.name.Contains("Terrain"))
        {
            //Output the message
            m_animator.SetBool("Landed",false);
            m_animator.SetBool("Jump",true);
            onFloor = false;
        }
    }
    /*
    [ServerRpc]
    private void SendMovementServerRpc(Vector3 movement){
        m_rigidbody.AddForce(movement,ForceMode.Impulse);
        
    }
    [ServerRpc]
    private void JumpServerRpc(){
        m_rigidbody.velocity = new Vector3(m_rigidbody.velocity.x,0,m_rigidbody.velocity.z);
        m_rigidbody.AddForce(transform.up * jumpHeight,ForceMode.Impulse);
    }*/
    // Update is called once per frame
    void FixedUpdate() 
    {
        if(IsOwner){
            groundedDrag();
            Jump();
            buttonWatcher();
            speedControl();
            //drawFloor();
            checkCrouch();
        }
        
        //checkGroundRay();
        
    }
}

