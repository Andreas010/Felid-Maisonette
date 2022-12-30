using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SetActiveIfLocalPlayer : MonoBehaviour
{

    [SerializeField] GameObject[] objs;
    [SerializeField] NetworkIdentity identity;

    void Start()
    {
        if(NetworkClient.localPlayer != identity)
        {
            foreach(GameObject t_go in objs)
            {
                t_go.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject t_go in objs)
            {
                t_go.SetActive(true);
            }
        }
    }
}
