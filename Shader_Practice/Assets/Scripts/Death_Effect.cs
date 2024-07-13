using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death_Effect : MonoBehaviour
{
    // Start is called before the first frame update
    public bool dead;
    public GameObject Arm_Right;
    public GameObject Arm_Left;
    public GameObject Leg_Right;
    public GameObject Leg_Left;
    public GameObject Torso;
    public GameObject Head;
    public ParticleSystem blood;
    public ParticleSystem bone;

    GameObject[] bodyParts;
    void Start()
    {
        dead = false;
        bodyParts = new GameObject[6];
        bodyParts[0] = Arm_Right;
        bodyParts[1] = Arm_Left;
        bodyParts[2] = Leg_Right;
        bodyParts[3] = Leg_Left;
        bodyParts[4] = Torso;
        bodyParts[5] = Head;
    }

    // Update is called once per frame
    void Update()
    {
        if(dead){
            Instantiate(blood,transform.position+Vector3.up,Quaternion.identity);
            Instantiate(bone,transform.position+Vector3.up,Quaternion.identity);
            for(int i = 0 ;i<bodyParts.Length;i++){
                GameObject temp = Instantiate(bodyParts[i],transform);
                temp.gameObject.transform.SetParent(null);
                temp.GetComponent<Rigidbody>().velocity = 10 * Random.insideUnitCircle.normalized;
            }
            dead = false;
            Destroy(this.gameObject);
        }
    }
}
