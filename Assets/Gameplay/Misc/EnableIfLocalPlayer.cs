using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnableIfLocalPlayer : NetworkBehaviour
{

    [SerializeField] Behaviour[] components;
    [SerializeField] Behaviour[] components_StayEnabledOnServer;
    [SerializeField] NetworkIdentity identity;

    void Start()
    {
        if (identity)
        {
            if (isServer) foreach (Behaviour t_c in components_StayEnabledOnServer) t_c.enabled = true;

            if (NetworkClient.localPlayer != identity)
            {
                foreach (Behaviour t_c in components) t_c.enabled = false;
                if (!isServer) foreach (Behaviour t_c in components_StayEnabledOnServer) t_c.enabled = false;
            }
            else
            {
                foreach (Behaviour t_c in components) t_c.enabled = true;
                if (!isServer) foreach (Behaviour t_c in components_StayEnabledOnServer) t_c.enabled = true;
            }
        }
        else
        {
            Debug.LogError("EnableIfLocalPlayer.cs has no identity specified! Use this to determine which player to check is the local player. Error game object: " + gameObject.name);
        }
    }
}
