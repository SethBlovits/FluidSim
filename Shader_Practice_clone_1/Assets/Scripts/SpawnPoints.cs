using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnPoints : NetworkBehaviour
{
    public override void OnNetworkSpawn(){
        if(IsClient && IsLocalPlayer){
           
            var clientId = NetworkManager.Singleton.LocalClient.ClientId;
            transform.position = new Vector3(25+clientId,25,25);
            //SetClientPositionServerRpc();
        }
        /*
        else if(IsOwner && !IsServer){
            SetClientPositionServerRpc();
        }*/
        
        
    }
    
    [ServerRpc]
    private void SetClientPositionServerRpc(){
        var clientId = NetworkManager.Singleton.LocalClient.ClientId;
        transform.position = new Vector3(25+clientId,25,25);
    }
}
