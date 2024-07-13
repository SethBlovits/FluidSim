using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Control : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject TestProjectile;
    public GameObject ArmEnd;
    public Camera cam;

    public GameObject currentProjectile;
    public GameObject fishing_Rod;
    public GameObject[] fish;
    public int[] fish_count;
    public GameObject hookedFish;
    void Start()
    {
        //projectileAvailable = false;
        fish = new GameObject[5];
        fish[0] = fishing_Rod;
        fish_count = new int[5];
    }

    // Update is called once per frame
    void addFish(){
        for(int i = 1; i<fish.Length;i++){
            if(hookedFish == fish[i] && hookedFish){
                fish_count[i]++;
                hookedFish = null;
                return;
            }
            if(fish[i] == null && hookedFish){
                fish[i] = hookedFish;
                fish_count[i]++;
                hookedFish = null;
                return;
            }
        }
        
    }
    void Update()
    {
        addFish();
        if(Input.GetKeyDown(KeyCode.Alpha2) && !currentProjectile){
            if(fish[1]){
                currentProjectile = Instantiate(fish[1],ArmEnd.transform);
                fishing_Rod.SetActive(false);
            }
            //projectileAvailable = true;
        }
        if(Input.GetKeyDown(KeyCode.Alpha3) && !currentProjectile){
            if(fish[2]){
                currentProjectile = Instantiate(fish[2],ArmEnd.transform);
                fishing_Rod.SetActive(false);
            }
            //projectileAvailable = true;
        }
        if(Input.GetKeyDown(KeyCode.Alpha4) && !currentProjectile){
            if(fish[3]){
                currentProjectile = Instantiate(fish[3],ArmEnd.transform);
                fishing_Rod.SetActive(false);
            }
            //projectileAvailable = true;
        }
        if(Input.GetKeyDown(KeyCode.Alpha5) && !currentProjectile){
            if(fish[4]){
                currentProjectile = Instantiate(fish[4],ArmEnd.transform);
                fishing_Rod.SetActive(false);
            }
            //projectileAvailable = true;
        }
        if(Input.GetKeyDown(KeyCode.Alpha1)){
            if(currentProjectile){
                Destroy(currentProjectile);
                currentProjectile = null;
            }
            fishing_Rod.SetActive(true);
        }
        if(Input.GetKeyDown(KeyCode.Mouse0) && currentProjectile){
            currentProjectile.GetComponent<Projectile_Logic>().enabled = true;
            currentProjectile.GetComponent<Projectile_Logic>().cam = cam;
            for(int i = 1; i<fish.Length;i++){
                if(fish[i]){
                    
                    if(currentProjectile.name.Contains(fish[i].name)){
                        fish_count[i]--;
                    }
                    if(fish_count[i] == 0){
                        fish[i] = null;
                    }
                }
            }
            currentProjectile.transform.SetParent(null);
            currentProjectile = null;
            fishing_Rod.SetActive(true);
        }

    }
}
