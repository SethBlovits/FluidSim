using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class fishing_buttons : NetworkBehaviour
{
    // Start is called before the first frame update
    public Animator m_animator;
    public Animator hand_animator;
    public GameObject fishHook;
    public GameObject cam;
    public GameObject endPoint;
    public GameObject player;
    public GameObject arm;
    public GameObject tempFish;
    float castTimer;
    float reelTimer;
    public GameObject hookClone;
    public GameObject serverHookClone;
    
    public LineRenderer lineRenderer;
    void Start(){
        castTimer = reelTimer = 0f;
    }
    [ServerRpc]
    public void spawnHookServerRpc(Vector3 position, Vector3 forward, Vector3 velocity){
        SpawnHookClientRpc(position,forward, velocity);
    }
    [ServerRpc]
    public void syncPositionServerRpc(Vector3 position){
        SyncHookPositionClientRpc(position);
    }
    [ClientRpc]
    public void SpawnHookClientRpc(Vector3 position, Vector3 forward, Vector3 velocity){
        if(IsOwner){
            return;
        }
        hookClone = Instantiate(fishHook,position+forward,Quaternion.identity);
        hookClone.GetComponent<Rigidbody>().velocity = velocity;

    }
    [ClientRpc]
    public void SyncHookPositionClientRpc(Vector3 position){
        if(IsOwner){
            return;
        }
        hookClone.transform.position = position;
    }
    [ServerRpc]
    public void destroyHookServerRpc(){
        destroyHookClientRpc();
    }
    [ClientRpc]
    public void destroyHookClientRpc(){
        if(IsOwner){
            return;
        }
        Destroy(hookClone);
    }
    [ClientRpc]
    public void setOtherPlayerVelocityClientRpc(Vector3 currentPlayerPosition, Vector3 otherPlayerPosition,ulong clientId,ClientRpcParams clientRpcParams = default){
        if(IsOwner){
            return;
        }
        NetworkManager.LocalClient.PlayerObject.gameObject.GetComponent<Rigidbody>().AddForce(currentPlayerPosition-otherPlayerPosition,ForceMode.VelocityChange);
        
    }
    private void fireHook(){
        hookClone = Instantiate(fishHook,cam.transform.position+cam.transform.forward,Quaternion.identity);
        //hookClone.GetComponent<NetworkObject>().Spawn();
        hookClone.GetComponent<Rigidbody>().velocity = 30*cam.transform.forward;
    }
    private void destroyHook(){
        //hookClone.GetComponent<NetworkObject>().Despawn();
        Destroy(hookClone);
    }
    // Update is called once per frame
    public void clickAnimation(){
        m_animator.SetBool("Recall",false);
        hand_animator.SetBool("Recall",false);
        castTimer -= Time.deltaTime;
        reelTimer -= Time.deltaTime;
        if(Input.GetKey(KeyCode.Mouse1)){
            //Debug.Log("Activated");
            
            if(hookClone && reelTimer<=0){
                m_animator.SetBool("Cast",false);
                m_animator.SetBool("Recall",true);
                hand_animator.SetBool("Cast",false);
                hand_animator.SetBool("Recall",true);
                if(hookClone.GetComponent<fishingHookStick>().inWater){
                    arm.GetComponent<Projectile_Control>().hookedFish = tempFish;
                }
                if(hookClone.GetComponent<fishingHookStick>().otherPlayer){
                    setOtherPlayerVelocityClientRpc(player.transform.position,hookClone.GetComponent<fishingHookStick>().otherPlayer.transform.position,
                    hookClone.GetComponent<fishingHookStick>().otherPlayer.GetComponent<NetworkObject>().OwnerClientId, new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] {hookClone.GetComponent<fishingHookStick>().otherPlayer.GetComponent<NetworkObject>().OwnerClientId }
                        }
                    });
                }
                else{
                    player.GetComponent<Rigidbody>().AddForce(hookClone.transform.position-player.transform.position,ForceMode.VelocityChange);
                }
                //Destroy(hookClone);
                destroyHook();
                destroyHookServerRpc();
                //destroyHookServerRpc();
                reelTimer = 1f;
                castTimer = .25f;
                
                
            }
            else{
                if(castTimer <= 0f){
                    m_animator.SetBool("Cast",true);
                    hand_animator.SetBool("Cast",true);
                    //hookClone = Instantiate(fishHook,cam.transform.position+cam.transform.forward,Quaternion.identity);
                    //hookClone.GetComponent<Rigidbody>().velocity = 30*cam.transform.forward;
                    //fireHookServerRpc();
                    fireHook();
                    spawnHookServerRpc(hookClone.transform.position,hookClone.transform.forward,hookClone.GetComponent<Rigidbody>().velocity);
                    //fireHookServerRpc();
                    //var instanceNetworkObject = hookClone.GetComponent<NetworkObject>();
                    //instanceNetworkObject.Spawn();
                    castTimer = 3f;
                    reelTimer = 1f; 
                }
            }
           
        }
        if(hookClone){
            Vector3[] linePoints = new Vector3[2];
            linePoints[0] = endPoint.transform.position;
            linePoints[1] = hookClone.transform.position;  
            lineRenderer.SetPositions(linePoints);
        }
        else{
            Vector3[] linePoints = new Vector3[2];
            linePoints[0] = endPoint.transform.position;
            linePoints[1] = endPoint.transform.position;  
            lineRenderer.SetPositions(linePoints);
        }
    }
    void Update()
    {
        if(IsOwner){
            clickAnimation();
        }
        if(hookClone){
            syncPositionServerRpc(hookClone.transform.position);
        }
        
        
    }
}
