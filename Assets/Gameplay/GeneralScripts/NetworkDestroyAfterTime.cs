using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkDestroyAfterTime : NetworkBehaviour
{

    [SerializeField] float timeBeforeDestroy;

    private void Start()
    {
        Invoke("Die", timeBeforeDestroy);
    }

    [Server]
    void Die()
    {
        Destroy(gameObject);
    }
}
