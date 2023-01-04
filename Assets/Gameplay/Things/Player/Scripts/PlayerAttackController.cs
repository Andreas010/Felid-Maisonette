using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PlayerAttackController : NetworkBehaviour
{

    #region per-move
    Vector2 currentAttackColPos;
    Vector2 currentAttackColSize;
    [SerializeField] Vector3 attackColliderOffset; // offsets the collider
    [SerializeField] float currentKnockbackTime;
    #endregion

    #region per-weapon
    [System.NonSerialized] public string currentWeapon = "Rusty Sword";
    int currentWeaponComboLength = 4; // Each weapon item should have this stat - ammount of attacks in your M1 combo
    [SerializeField] float currentDamage;
    [SerializeField] Vector2 currentKnockbackForce;
    #endregion

    public bool weaponOut;

    [SerializeField] bool debug;
    int indexInCombo;
    float timeSinceLastAttack;

    [SerializeField] float comboAttackWindow = 1; // Ammount of time after your last attack that you will be able to combo another move
    [SerializeField] float attackStartVelocityMod; // Modification to the player's velocity once an attack begins in order to stop sliding when attacking ex: .5f (slows velocity by half when you begin attacking)

    GameObject[] weaponObjects; // 0 = weapon on back, 1 = weapon held
    PlayerController playerController;
    NetworkPlayer networkPlayer;
    Transform weaponRoot; // The object that is the parent of the weapon held, ie: weaponObjects[1]
    NetworkIdentity identity;
    Animator animator;
    [SerializeField] LayerMask entityLayer;
    BoxCollider2D debugCollider;

    Controls input;

    void Start()
    {
        //debugCollider = Instantiate(debugColldierObj, Vector3.zero, Quaternion.identity).GetComponent<BoxCollider2D>();

        GameObject t_atkDebugCol = Instantiate(new GameObject("- ATTACK DEBUG COLLIDER - (" + name + ")"));
        debugCollider = t_atkDebugCol.AddComponent<BoxCollider2D>(); debugCollider.isTrigger = true;

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

        if (weaponOut) 
        {
            if (playerController.facingRight) weaponRoot.localScale = new Vector3(1, 1, 1);
            else                              weaponRoot.localScale = new Vector3(-1, 1, 1);
        }

        if (playerController.attacking == false) timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack > comboAttackWindow) indexInCombo = 0;

        if(debug)
        {
            debugCollider.offset = CalculateAttackColPos();
            debugCollider.size = currentAttackColSize;
        }
    }

    #region Input

    void InitControls()
    {
        input = GameManager.Singleton.Input;
        input.World.Enable();

        input.World.ToggleWeapon.performed += ToggleWeapon;
        input.World.Attack.performed += Attack;
        input.World.AttackM2.performed += AttackM2;
    }


    void ToggleWeapon(InputAction.CallbackContext t_context)
    {
        weaponOut = !weaponOut;
        networkPlayer.ToggleWeapon_Server(identity, weaponOut);
    }

    void AttackM2(InputAction.CallbackContext t_context)
    {
        if (StartAttack() == false) return; // If you cannot currently attack

        if (!weaponOut)
        {
            currentAttackColPos = new Vector2(1f, -.4f);
            currentAttackColSize = new Vector2(1.8f, 2f);
            currentKnockbackForce = new Vector2(2, 12);
            currentKnockbackTime = 1.5f;

            animator.SetTrigger("Uppercut");
        }
        else
        {
            switch (currentWeapon)
            {
                case "Rusty Sword":
                    {
                        currentAttackColPos = new Vector2(1f, -.4f);
                        currentAttackColSize = new Vector2(1.8f, 2f);
                        currentKnockbackForce = new Vector2(12, 4);
                        currentKnockbackTime = 1.5f;

                        animator.SetTrigger("Rusty Sword_Thrust");
                        break;
                    }
                default:
                    {
                        EndAttack();
                        break;
                    }
            }
        }

        indexInCombo = 0;
    }

    void Attack(InputAction.CallbackContext t_context)
    {
        if (StartAttack() == false) return; // If you cannot currently attack

        if (!weaponOut)
        {
            currentAttackColPos = new Vector2(.7f, -.4f);
            currentAttackColSize = new Vector2(2.3f, 2f);
            currentKnockbackForce = new Vector2(3.5f, 2);
            currentKnockbackTime = .5f;

            if (indexInCombo == 0)
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
                currentAttackColPos = new Vector2(.7f, -.4f);
                currentAttackColSize = new Vector2(2.3f, 2f);
                currentKnockbackForce = new Vector2(2, 12);
                currentKnockbackTime = 1.5f;

                animator.SetTrigger("Uppercut");
            }
        }
        else
        {
            switch (currentWeapon)
            {
                case "Rusty Sword":
                    {
                        currentAttackColPos = new Vector2(.7f, -.4f);
                        currentAttackColSize = new Vector2(2.3f, 2f);
                        currentKnockbackForce = new Vector2(3.5f, 2);
                        currentKnockbackTime = .5f;

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
                            currentAttackColPos = new Vector2(.7f, -.4f);
                            currentAttackColSize = new Vector2(2.3f, 2f);
                            currentKnockbackForce = new Vector2(12, 4);
                            currentKnockbackTime = 1.5f;

                            animator.SetTrigger("Rusty Sword_Thrust");
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

    bool StartAttack()
    {
        if (!playerController.canMove || playerController.attacking) return false;
        playerController.attacking = true;
        playerController.ChangePhysicsMat(1);
        Vector2 t_rigVel = playerController.rig.velocity;
        playerController.rig.velocity = new Vector2(t_rigVel.x * attackStartVelocityMod, t_rigVel.y);

        if (timeSinceLastAttack < comboAttackWindow) indexInCombo++;
        return true;
    }

    public void SendAttack()
    {
        if (!isLocalPlayer) return;
        Command_SendAttack(CalculateAttackColPos(), currentAttackColSize, currentKnockbackForce);
    }

    [Command]
    public void Command_SendAttack(Vector3 t_finalAttackColPos, Vector2 t_finalAttackColSize , Vector2 t_currentKnockbackForce)
    {
        Collider2D[] t_hitColliders;

        t_hitColliders = Physics2D.OverlapBoxAll(t_finalAttackColPos, t_finalAttackColSize, 0, entityLayer);

        if (t_hitColliders.Length <= 0) return;
        foreach(Collider2D t_collider in t_hitColliders)
        {
            Debug.Log(t_collider.gameObject.name);
            if (t_collider.CompareTag("Entity") && t_collider.GetComponent<NetworkIdentity>() != identity)
            {
                Vector2 t_finalKnockbackForce = t_currentKnockbackForce;
                if (t_collider.transform.position.x < transform.position.x) t_finalKnockbackForce = new Vector2(-t_currentKnockbackForce.x, t_currentKnockbackForce.y);

                t_collider.GetComponent<Entity>().TakeDamage(currentDamage, t_finalKnockbackForce, currentKnockbackTime);
            }
        }
    }

    Vector3 CalculateAttackColPos()
    {
        if (playerController.facingRight) return transform.position + new Vector3(currentAttackColPos.x, currentAttackColPos.y, 0) + attackColliderOffset;
        else return transform.position + new Vector3(currentAttackColPos.x * -1, currentAttackColPos.y, 0) + attackColliderOffset;
    }

    public void EndAttack()
    {
        if (!isLocalPlayer) return;

        if (indexInCombo >= currentWeaponComboLength - 1) { timeSinceLastAttack = 99; indexInCombo = 0; }
        else timeSinceLastAttack = 0;
        playerController.ChangePhysicsMat(0);
        playerController.attacking = false;
    }

}
