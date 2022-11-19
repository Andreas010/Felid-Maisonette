using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region assignables
    [Header("Assignables")]
    Rigidbody2D rig;
    [SerializeField] CircleCollider2D groundCheckCol;
    [SerializeField] TriggerCheck groundCheck;
    [SerializeField] Transform visuals;

    Controls input;
    Vector2 joy;
    #endregion

    #region stats
    [Header("Stats")]
    [SerializeField] float walkSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float jumpForce;
    [SerializeField] float counterJump;
    #endregion

    bool grounded;
    bool sprinting;
    bool jumpHeld; bool jumping;

    float curSpeed;

    #region unityMethods
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        InitControls();
    }

    void Update()
    {
        joy = new Vector2(input.World.Horizontal.ReadValue<float>(), input.World.Vertical.ReadValue<float>());
        if (sprinting) curSpeed = runSpeed; else curSpeed = walkSpeed;
        grounded = groundCheck.overlapping;


        ApplyHorizontalInput(joy.x);
        if (joy.x > .5f) visuals.localScale = Vector3.one;
        if (joy.x < -.5f) visuals.localScale = new Vector3(-1, 1, 1);
    }

    private void FixedUpdate()
    {
        if (jumping)
        {
            if (!jumpHeld)// && Vector2.Dot(rig.velocity, Vector2.up) > 0)
            {
                //jumping = false;
            }
        }
    }
    #endregion

    #region other methods
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

    #region input
    void InitControls()
    {
        input = GameManager.Singleton.Input;
        input.World.Enable();

        input.World.Jump.performed += Jump;
        input.World.Jump.canceled += CancelJump;

        input.World.Sprint.performed += Sprint;
        input.World.Sprint.canceled += CancelSprint;
    }

    public void Jump(InputAction.CallbackContext t_context) 
    {
        jumpHeld = true;
        if (grounded)
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
        else rig.velocity = new Vector2(rig.velocity.x, rig.velocity.y * 2);
    }

    public void Sprint(InputAction.CallbackContext t_context) { sprinting = true; }
    public void CancelSprint(InputAction.CallbackContext t_context) { sprinting = false; }
    #endregion
    #endregion
}
