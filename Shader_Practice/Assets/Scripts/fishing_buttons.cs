using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fishing_buttons : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator m_animator;
    public GameObject fishHook;
    public GameObject cam;
    public GameObject endPoint;
    public GameObject player;
    float castTimer;
    float reelTimer;
    GameObject hookClone;
    
    public LineRenderer lineRenderer;
    void Start(){
        castTimer = reelTimer = 0f;
    }
    // Update is called once per frame
    void Update()
    {
        m_animator.SetBool("Recall",false);
        castTimer -= Time.deltaTime;
        reelTimer -= Time.deltaTime;
        if(Input.GetKey(KeyCode.Mouse1)){
            //Debug.Log("Activated");
            
            if(hookClone && reelTimer<=0){
                m_animator.SetBool("Cast",false);
                m_animator.SetBool("Recall",true);
                //player.GetComponent<Rigidbody>().velocity += hookClone.transform.position-player.transform.position;
                Destroy(hookClone);
                reelTimer = 1f;
                castTimer = .25f;
                
                
            }
            else{
                if(castTimer <= 0f){
                    m_animator.SetBool("Cast",true);
                    hookClone = Instantiate(fishHook,cam.transform.position+cam.transform.forward,Quaternion.identity);
                    hookClone.GetComponent<Rigidbody>().velocity = 30*cam.transform.forward;
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
}
