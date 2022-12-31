using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PlayerAttackController : NetworkBehaviour
{

    [System.NonSerialized] public string currentWeapon;

    public bool weaponOut;

    GameObject[] weaponObjects; // 0 = weapon on back, 1 = weapon held
    PlayerController playerController;
    NetworkPlayer networkPlayer;
    Transform weaponRoot;
    NetworkIdentity identity;

    Controls input;

    void Start()
    {
        networkPlayer = GetComponent<NetworkPlayer>();
        weaponObjects = networkPlayer.weaponObjects;
        weaponRoot = weaponObjects[1].transform.parent;
        playerController = GetComponent<PlayerController>();
        identity = GetComponent<NetworkIdentity>();

        networkPlayer.ToggleWeapon_Server(identity, weaponOut);
        InitControls();
    }

    void Update()
    {
        //weaponObjects[0].SetActive(!t_newWeaponOut);
        //weaponObjects[1].SetActive(t_newWeaponOut);

        if (weaponOut) 
        {
            if (playerController.facingRight) weaponRoot.localScale = new Vector3(1, 1, 1);
            else                              weaponRoot.localScale = new Vector3(-1, 1, 1);
        }
    }

    void InitControls()
    {
        input = GameManager.Singleton.Input;
        input.World.Enable();

        input.World.ToggleWeapon.performed += ToggleWeapon;
    }

    void ToggleWeapon(InputAction.CallbackContext t_context)
    {
        weaponOut = !weaponOut;
        networkPlayer.ToggleWeapon_Server(identity, weaponOut);
    }


}
