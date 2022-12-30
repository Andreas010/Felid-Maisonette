using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnableIfLocalPlayer : NetworkBehaviour
{

    [SerializeField] Behaviour[] components;
    [SerializeField] NetworkIdentity identity;

    void Start()
    {
        if (identity)
        {
            if (NetworkClient.localPlayer != identity)
            {
                foreach (Behaviour t_c in components)
                {
                    t_c.enabled = false;
                }
            }
            else
            {
                foreach (Behaviour t_c in components)
                {
                    t_c.enabled = true;
                }
            }
        }
        else
        {
            Debug.LogError("EnableIfLocalPlayer.cs has no identity specified! Use this to determine which player to check is the local player. Error game object: " + gameObject.name);
        }
    }
}
