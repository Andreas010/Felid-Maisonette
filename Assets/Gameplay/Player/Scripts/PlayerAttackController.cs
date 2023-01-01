using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PlayerAttackController : NetworkBehaviour
{

    [System.NonSerialized] public string currentWeapon = "Rusty Sword";
    int currentWeaponComboLength = 4; // Each weapon item should have this stat - ammount of attacks in your M1 combo

    public bool weaponOut;

    int indexInCombo;
    float timeSinceLastAttack;

    [SerializeField] float comboAttackWindow = 1; // Ammount of time after your last attack that you will be able to combo another move
    [SerializeField] float attackStartVelocityMod; // Modification to the player's velocity once an attack begins in order to stop sliding when attacking ex: .5f (slows velocity by half when you begin attacking)

    GameObject[] weaponObjects; // 0 = weapon on back, 1 = weapon held
    PlayerController playerController;
    NetworkPlayer networkPlayer;
    Transform weaponRoot;
    NetworkIdentity identity;
    Animator animator;

    Controls input;

    void Start()
    {
        networkPlayer = GetComponent<NetworkPlayer>();
        weaponObjects = networkPlayer.weaponObjects;
        weaponRoot = weaponObjects[1].transform.parent;
        playerController = GetComponent<PlayerController>();
        identity = GetComponent<NetworkIdentity>();
        animator = GetComponent<Animator>();

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

        if (playerController.attacking == false) timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack > comboAttackWindow) indexInCombo = 0;
    }

    public void EndAttack()
    {
        if (indexInCombo >= currentWeaponComboLength - 1) { timeSinceLastAttack = 99; indexInCombo = 0; }
        else timeSinceLastAttack = 0;
        playerController.ChangePhysicsMat(0);
        playerController.attacking = false;
    }

    #region Input

    void InitControls()
    {
        input = GameManager.Singleton.Input;
        input.World.Enable();

        input.World.ToggleWeapon.performed += ToggleWeapon;
        input.World.Attack.performed += Attack;
    }


    void ToggleWeapon(InputAction.CallbackContext t_context)
    {
        weaponOut = !weaponOut;
        networkPlayer.ToggleWeapon_Server(identity, weaponOut);
    }

    void Attack(InputAction.CallbackContext t_context)
    {
        if (!playerController.canMove || playerController.attacking) return;
        playerController.attacking = true;
        playerController.ChangePhysicsMat(1);
        Vector2 t_rigVel = playerController.rig.velocity;
        playerController.rig.velocity = new Vector2(t_rigVel.x * attackStartVelocityMod, t_rigVel.y);

        if (timeSinceLastAttack < comboAttackWindow) indexInCombo++;

        if (!weaponOut)
        {
            if(indexInCombo == 0)
            {
                animator.SetTrigger("LeftPunch");
            }
            else if(indexInCombo == 1)
            {
                animator.SetTrigger("RightPunch");
            }
            else if (indexInCombo == 2)
            {
                animator.SetTrigger("LeftPunch");
            }
            else if (indexInCombo == 3)
            {
                animator.SetTrigger("Uppercut");
            }
        }
        else
        {
            switch (currentWeapon)
            {
                case "Rusty Sword":
                    {
                        if (indexInCombo == 0)
                        {
                            animator.SetTrigger("Rusty Sword_SlashDown");
                        }
                        else if (indexInCombo == 1)
                        {
                            animator.SetTrigger("Rusty Sword_SlashUp");
                        }
                        else if (indexInCombo == 2)
                        {
                            animator.SetTrigger("Rusty Sword_SlashDown");
                        }
                        else if (indexInCombo == 3)
                        {
                            animator.SetTrigger("Uppercut");
                        }
                        break;
                    }
                default:
                    {
                        EndAttack();
                        break;
                    }
            }
        }

    }

    #endregion


}
