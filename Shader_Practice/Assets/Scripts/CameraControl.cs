using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
   public Camera cam;
    public float vert_sens;
    public float horiz_sens;
    Vector3 currentAngle  = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        cam.transform.rotation = transform.rotation;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float horiz  = horiz_sens * Input.GetAxis("Mouse X");
        float vert = -vert_sens * Input.GetAxis("Mouse Y");
        
        /*if(cam.transform.rotation.eulerAngles.y>80 && vert>0){
            currentAngle += new Vector3(0,horiz,0);
        }
        else if(cam.transform.rotation.eulerAngles.y>-80 && vert<0){
            currentAngle += new Vector3(0,horiz,0);
        }
        else{
            currentAngle += new Vector3(vert,horiz,0);
            Debug.Log(cam.transform.rotation.y);
            Debug.Log(cam.transform.rotation.eulerAngles.y); 
        }*/
        currentAngle += new Vector3(vert,horiz,0);
        cam.transform.rotation = Quaternion.Euler(currentAngle);
        //player.transform.Rotate(0,horiz,0);
        //float cameraY =  transform.rotation.y;
        transform.rotation = Quaternion.Euler(0,currentAngle.y,0);
        //cam.transform.position = transform.position + new Vector3(x_camera_adjust,y_camera_adjust,0);
    }
}
