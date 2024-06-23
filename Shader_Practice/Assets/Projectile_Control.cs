using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Control : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject TestProjectile;
    public GameObject ArmEnd;
    public Camera cam;

    GameObject currentProjectile;
    public GameObject fishing_Rod;
    void Start()
    {
        //projectileAvailable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha2) && !currentProjectile){
            currentProjectile = Instantiate(TestProjectile,ArmEnd.transform);
            fishing_Rod.SetActive(false);
            //projectileAvailable = true;
        }
        if(Input.GetKeyDown(KeyCode.Mouse0) && currentProjectile){
            currentProjectile.GetComponent<Projectile_Logic>().enabled = true;
            currentProjectile.GetComponent<Projectile_Logic>().cam = cam;
            currentProjectile = null;
            fishing_Rod.SetActive(true);
        }

    }
}
