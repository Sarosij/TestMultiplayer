using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using AgoraChat;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local {get; set;}

    [Networked] public bool isLocalPlayer { get; private set; }

    //Agora Temp token and user Id
    [Networked] public string UserID { get; private set; }
    [Networked] public string Token { get; private set; }
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            Debug.Log("Local Player Spawned");

            // Generate a unique user ID (e.g., using a GUID or random number)
            UserID = "User" + Random.Range(1, 10000);
            Debug.Log("Generated UserID: " + UserID);

            
        }
        else
        {
            Debug.Log("Client Player Spawned");
            UserID = "User" + Random.Range(1, 10000);
            Debug.Log("Generated UserID: " + UserID);
        }
    }
    public void PlayerLeft(PlayerRef player)
    {
        if(player == Object.HasInputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
