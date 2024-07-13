using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BodyVisibility : NetworkBehaviour
{
    // Start is called before the first frame update
    public void Start(){
        if(!IsLocalPlayer){
            this.gameObject.layer = 0;
        }
        
    }
}
