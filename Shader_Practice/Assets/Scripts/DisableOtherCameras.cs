using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DisableOtherCameras : NetworkBehaviour
{
    // Start is called before the first frame update
    public  override void OnNetworkSpawn(){
        if(!IsOwner){
            this.gameObject.SetActive(false);
        }
    }
}
