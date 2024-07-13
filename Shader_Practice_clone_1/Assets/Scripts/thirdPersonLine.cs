using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class thirdPersonLine : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject fishing_rod;
    public LineRenderer lineRenderer;
    public GameObject endPoint;
    
    // Update is called once per frame
    void Update()
    {
        if(fishing_rod.GetComponent<fishing_buttons>().hookClone){
            Vector3[] linePoints = new Vector3[2];
            linePoints[0] = endPoint.transform.position;
            linePoints[1] = fishing_rod.GetComponent<fishing_buttons>().hookClone.transform.position;  
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
