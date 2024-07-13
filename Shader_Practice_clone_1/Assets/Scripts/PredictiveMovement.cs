using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PredictiveMovement : NetworkBehaviour
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
    Vector3 moveVector;

    private int tick = 0;
    private float tickRate = 1f/60f;
    private float tickDeltaTime = 0;
    private const int buffer = 1024;
    private HandleState.InputState[] inputStates = new HandleState.InputState[buffer];
    private HandleState.TransformStateRW[] transformStates = new HandleState.TransformStateRW[buffer];
    public NetworkVariable<HandleState.TransformStateRW> currentServerTransformState = new();
    public HandleState.TransformStateRW previousTransformState;

    private void OnServerStateChanged(HandleState.TransformStateRW previousValue, HandleState.TransformStateRW newValue){
        previousTransformState = previousValue;
    }
    public void OnEnable(){
        currentServerTransformState.OnValueChanged += OnServerStateChanged;
    }
   
    void buttonWatcher(){
        float moveSpeed = walkSpeed;
        moveVector = Vector3.zero;
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
            
            m_rigidbody.AddForce( moveVector,ForceMode.Impulse);
            //transform.position+= moveVector;
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
    public void localPlayerMovement(){
        tickDeltaTime += Time.deltaTime;
        if(tickDeltaTime > tickRate){
            int bufferIndex = tick % buffer;
            MovePlayerWithTickServerRpc(tick);
            buttonWatcher();
            
            HandleState.InputState inputState = new()
            {
                tick = tick,
                moveVector = moveVector
            };
            HandleState.TransformStateRW transformState = new(){
                tick = tick,
                finalPos = transform.position,
                finalVelocity = m_rigidbody.velocity,
                finalAngularVelocity = m_rigidbody.angularVelocity,
                finalRotation = transform.rotation,
                isMoving = true

            };
            inputStates[bufferIndex] = inputState;
            transformStates[bufferIndex] = transformState;

            tickDeltaTime -= tickRate;
            if(tick == buffer){
                tick = 0;
            }
            else{
                tick++;
            }
        }
    }
    public void SimulateOtherPlayers(){
        tickDeltaTime += Time.deltaTime;
        if(tickDeltaTime > tickRate){
            if(currentServerTransformState.Value != null && currentServerTransformState.Value.isMoving){
                transform.position = currentServerTransformState.Value.finalPos;
                transform.rotation = currentServerTransformState.Value.finalRotation;
                m_rigidbody.velocity = currentServerTransformState.Value.finalVelocity;
                m_rigidbody.angularVelocity = currentServerTransformState.Value.finalAngularVelocity;
            }
            tickDeltaTime -= tickRate;
            if(tick == buffer){
                tick = 0;
            }
            else{
                tick++;
            }
        }

    }
    [ServerRpc]
    public void MovePlayerWithTickServerRpc(int tick){
        buttonWatcher();
        HandleState.TransformStateRW transformState = new(){
            tick = tick,
            finalPos = transform.position,
            finalRotation = transform.rotation,
            finalAngularVelocity = m_rigidbody.angularVelocity,
            finalVelocity = m_rigidbody.velocity,
            isMoving = true

        };
        previousTransformState = currentServerTransformState.Value;
        currentServerTransformState.Value = transformState;
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
            
        }
    }
    void drawFloor(){
        Debug.DrawRay(transform.position+transform.up,-transform.up);
    }
    
    void OnTriggerEnter(Collider collision)
    {
        //Check to see if the Collider's name is "Chest"
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
   
    // Update is called once per frame
    void Update() 
    {
        if(IsClient && IsLocalPlayer){
            localPlayerMovement();
        }
        
        else{
            SimulateOtherPlayers();
        }

       /* if(IsOwner){
            groundedDrag();
            Jump();
            buttonWatcher();
            speedControl();
            //drawFloor();
            checkCrouch();
        }*/
        
        //checkGroundRay();
        
    }
}
