using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using System.Text.RegularExpressions;

public class PlayerController : NetworkBehaviour
{
    #region assignables
    [Header("Assignables")]
    [System.NonSerialized] public Rigidbody2D rig;
    [SerializeField] CircleCollider2D groundCheckCol;
    [SerializeField] TriggerCheck groundCheck;
    [SerializeField] Transform visuals;
    CapsuleCollider2D collider;
    [SerializeField] PhysicsMaterial2D[] physicsMaterials; // 0 = no friction, 1 = high friction
    Entity entityScript;

    [SerializeField] SpriteRenderer[] srs; //(index) 0 = head, 1 = torso, 2 = legs
    [SerializeField] int[] partSpriteIndexes = new int[3]; //(index) 0 = head, 1 = torso, 2 = legs (this variable is which frame of animation each part of the player is currently on)

    Animator animator;
    Controls input;
    Vector2 joy;
    #endregion

    #region stats
    [Header("Stats")]
    [SerializeField] float walkSpeed = 4.5f; [SerializeField] float runSpeed = 7.5f; [SerializeField] float acceleration = 25f;
    [SerializeField] float jumpForce = 11.5f; [SerializeField] float counterJump = 3; [SerializeField] float velYToApplyCounterJump; /* Y velocity when jumping that will automatically apply the counter jump */ [SerializeField] float jumpInputForgiveness = .5f; float timeSinceJump; bool counterJumpApplied;
    [SerializeField] float cyoteTime = .25f;
    [SerializeField] float dashForce = 14f; [SerializeField] float dashTime = .2f; [SerializeField] float maxDashCooldown = 1f; float dashCooldown; [SerializeField] float groundedDashModifier = 1.3f;
    #endregion

    [System.NonSerialized] public bool facingRight;
    bool grounded; float timeSinceGrounded;
    bool sprinting;
    bool jumpHeld; bool jumping;
    [System.NonSerialized] public bool attacking;

    float curSpeed;

    bool dashing; bool canDash;
    [System.NonSerialized] public bool canMove;

    #region unityMethods
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider2D>();
        entityScript = GetComponent<Entity>();
        InitControls();
    }

    void Update()
    {
        joy = new Vector2(input.World.Horizontal.ReadValue<float>(), input.World.Vertical.ReadValue<float>());
        if (sprinting) curSpeed = runSpeed; else curSpeed = walkSpeed;
        grounded = groundCheck.overlapping;

        canMove = !dashing && !attacking && !entityScript.takingKnockback;
        if(canMove)
        {
            ApplyHorizontalInput(joy.x);
            if (joy.x > .5f) { facingRight = true; visuals.localScale = Vector3.one; }
            if (joy.x < -.5f) { facingRight = false; visuals.localScale = new Vector3(-1, 1, 1); }

            if (jumping && rig.velocity.y < velYToApplyCounterJump && !counterJumpApplied) { counterJumpApplied = true; rig.velocity = new Vector2(rig.velocity.x, -counterJump); }

            if (grounded)
            {
                if (timeSinceJump > .25f) { jumping = false; counterJumpApplied = false; }
                if(timeSinceJump < jumpInputForgiveness && jumpHeld) { Jump(new InputAction.CallbackContext()); }
                canDash = true;
                if (joy.x > -.1f && joy.x < .1f && grounded) ChangePhysicsMat(1); else ChangePhysicsMat(0);
            }
            else
            {
                ChangePhysicsMat(0);
            }
        }
        else
        {
            if(!dashing) ChangePhysicsMat(1); else ChangePhysicsMat(0);
        }

        animator.SetBool("Grounded", grounded);
        animator.SetBool("Sprinting", sprinting);
        animator.SetBool("Attacking", attacking);
        animator.SetFloat("Magnitude", rig.velocity.magnitude);
        animator.SetFloat("XVel", rig.velocity.x);
        animator.SetFloat("YVel", rig.velocity.y);

        for (int i = 0; i < 3; i++) { partSpriteIndexes[i] = int.Parse(Regex.Replace(srs[i].sprite.name, "[A-Za-z ]", "").Trim('_')); }

        dashCooldown -= Time.deltaTime;
        timeSinceJump += Time.deltaTime;
        if (!grounded) timeSinceGrounded += Time.deltaTime; else timeSinceGrounded = 0;

    }
    #endregion

    #region movement methods
    void ApplyHorizontalInput(float input)
    {
        float targetVelocity = input * curSpeed;
        float currentVelocity = rig.velocity.x;
        float deltaVelocity = Mathf.Clamp(targetVelocity - currentVelocity, -curSpeed, curSpeed);

        // Sign
        // -1 if negative
        //  1 if positive
        float signCur = Mathf.Sign(currentVelocity);
        float signTarget = Mathf.Sign(targetVelocity);

        // If the directions are the same
        if (signCur == signTarget || targetVelocity == 0)
        {
            // And the current velocity is larger than our current one
            float absCur = Mathf.Abs(currentVelocity);
            float absTarget = Mathf.Abs(targetVelocity);
            if (absCur > absTarget && absCur > curSpeed) // return
                deltaVelocity *= 0.25f;
        }
        rig.velocity = new Vector2(currentVelocity + deltaVelocity * acceleration * Time.deltaTime, rig.velocity.y);
    }

    public void ApplyForwardForce(float t_force)
    {
        if (!isLocalPlayer) return;

        if (joy.y < -.5f) return;
        if(facingRight) rig.AddForce(transform.right * t_force, ForceMode2D.Impulse);
        else rig.AddForce(transform.right * -t_force, ForceMode2D.Impulse);
    }
    public void ApplyUpwardForce(float t_force)
    {
        if (!isLocalPlayer) return;

        rig.AddForce(transform.up * t_force, ForceMode2D.Impulse);
    }

    public void ChangePhysicsMat(int t_indexOfNewMat)
    {
        if (entityScript.takingKnockback) return;
        collider.sharedMaterial = physicsMaterials[t_indexOfNewMat];
    }

    #region input
    void InitControls()
    {
        input = GameManager.Singleton.Input;
        input.World.Enable();

        input.World.Jump.performed += Jump;
        input.World.Jump.canceled += CancelJump;

        input.World.Sprint.performed += Sprint;
        input.World.Sprint.canceled += CancelSprint;

        input.World.Dash.performed += Dash;

    }
    Vector2 JoyVector()
    {
        return new Vector2(joy.x, joy.y);
    }

    public void Jump(InputAction.CallbackContext t_context) 
    {
        timeSinceJump = 0;
        jumpHeld = true;
        if (canMove && (grounded || timeSinceGrounded < cyoteTime))
        {
            jumping = true;
            //rig.AddForce(Vector2.up * jumpForce * rig.mass, ForceMode2D.Impulse);
            rig.velocity = new Vector2(rig.velocity.x, jumpForce);// (Vector2.up * jumpForce * rig.mass, ForceMode2D.Impulse);
        }
    }
    public void CancelJump(InputAction.CallbackContext t_context) 
    { 
        jumpHeld = false;
        //if (rig.velocity.y > 5) rig.velocity = new Vector2(rig.velocity.x, 0);
        if (rig.velocity.y > -counterJump) rig.velocity = new Vector2(rig.velocity.x, -counterJump);
        //else if (rig.velocity.y > ) rig.velocity = new Vector2(rig.velocity.x, rig.velocity.y * 2);
    }

    public void Sprint(InputAction.CallbackContext t_context) { sprinting = true; }
    public void CancelSprint(InputAction.CallbackContext t_context) { sprinting = false; }

    #region dash
    public void Dash(InputAction.CallbackContext t_context)
    {
        if (dashCooldown > 0 || !canDash || attacking) return;
        dashing = true;
        canDash = false;

        float t_finalDashForce;
        if (grounded) t_finalDashForce = dashForce * groundedDashModifier; else t_finalDashForce = dashForce;

        animator.StopPlayback();
        animator.SetBool("Dashing", true);
        animator.SetTrigger("Roll");
        //if (!((joy.x > -.2f && joy.x < .2f) && joy.y > .2f)) animator.SetTrigger("Roll");
        if (joy.magnitude < .2f)
        {
            if (facingRight) rig.velocity = new Vector2(1, 0) * t_finalDashForce;
            else             rig.velocity = new Vector2(-1, 0) * t_finalDashForce;
        }
        else rig.velocity = JoyVector() * t_finalDashForce;
        Invoke("EndDash", dashTime);
    }
    public void EndDash()
    {
        rig.velocity *= .3f;
        dashing = false;
        dashCooldown = maxDashCooldown;
        if (!grounded) Invoke("EndDashAnimation", .25f);
        else Invoke("EndDashAnimation", .45f);
    }
    void EndDashAnimation() { animator.SetBool("Dashing", false); }
    #endregion

    #endregion
    #endregion

    #region network methods

    #endregion
}
