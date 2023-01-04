using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject[] weaponObjects; // 0 = weapon on back, 1 = weapon held

    #region Network Methods
    [Command]
    public void ToggleWeapon_Server(NetworkIdentity t_identity, bool t_weaponOut)
    {
        ToggleWeapon_ClientRPC(t_identity, t_weaponOut);
    }

    [ClientRpc]
    public void ToggleWeapon_ClientRPC(NetworkIdentity t_identity, bool t_weaponOut)
    {
        //t_identity.GetComponent<NetworkPlayer>().weaponObjects[0].SetActive(!t_weaponOut);
        //t_identity.GetComponent<NetworkPlayer>().weaponObjects[1].SetActive(t_weaponOut);

        weaponObjects[0].SetActive(!t_weaponOut);
        weaponObjects[1].SetActive(t_weaponOut);
    }
    #endregion
}
