using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D)),RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    public PlayerStats stats;
    private Rigidbody2D rb;
    private CapsuleCollider2D cCollider;
    private Vector2 inputAxis;
    private Vector2 velocity;

    [SerializeField]
    private bool isGrounded;
    private float timeSinceJumpPressed;
    private float timeSinceLeftGround;
    private bool canJump;
    private bool canCoyoteJump;
    private bool jumpCut;
    //------------------------------------
    private int jumpCount;
    [SerializeField]
    private bool isTouchingWall;
    [SerializeField]
    private bool isDashing;
    private float dashTimeLeft;
    private float lastDash = -50.0f;
    //------------------------------------
    public event Action<bool> OnGroundedChange;
    public event Action OnJump;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        ProcessInput();
        //---------
        ProcessDash();
        //------------
    }

    private void FixedUpdate()
    {
        //------
        CheckWallStatus();
        //---------
        CheckGroundStatus();
        ApplyJump();
        ApplyMovement();
        

        //------
        if (isDashing)
            ApplyDash();
        //--------
    }

    void ProcessInput()
    {
        inputAxis.x = Input.GetAxisRaw("Horizontal");
        inputAxis.y = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            canJump = true;
            timeSinceJumpPressed = Time.time;
        }
        //---------
        if (Input.GetButtonDown("Dash")&&Time.time >= (lastDash+stats.dashCoolDown))
        {
            AttemptToDash();
        }
        //----------
    }

    void CheckGroundStatus()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.CapsuleCast(cCollider.bounds.center,cCollider.size,
            cCollider.direction,0,Vector2.down,stats.GroundDistance,stats.groundLayer);
        //-----------------
        if (isGrounded)
            jumpCount = 0;
        //-------------------------
        if (isGrounded != wasGrounded)
        {
            OnGroundedChange?.Invoke(isGrounded);
            if (isGrounded)
            {
                canCoyoteJump = true;
                jumpCut = false;
            }
        }
        else
        {
            timeSinceLeftGround = Time.time;
        }
    }
  
    void ApplyMovement()
    {
        float targetSpeed = inputAxis.x * stats.maxSpeed;
        rb.velocity = new Vector2(targetSpeed,rb.velocity.y);
    }

    void ApplyJump()
    {
        //---------

        //Double Jump Logic
        if (canJump && jumpCount < stats.maxJumpCount)
        {
            velocity.y = stats.jumpPower;
            rb.velocity = new Vector2(rb.velocity.x, velocity.y);
            jumpCount++;
            canJump = false;
            OnJump?.Invoke();
        }//Wall Jump Logic
        else if (isTouchingWall && !isGrounded && inputAxis.x != 0)
        {

            float wallJumpDir = (inputAxis.x > 0) ? -1 : 1;
            rb.AddForce(new Vector2(wallJumpDir * stats.wallJumpForce, stats.wallJumpPower), ForceMode2D.Impulse);
            jumpCount = 0;

            StartCoroutine(DisableWallJump(stats.tempDisableDuration));
        }

        //---------

        if ((canJump || (canCoyoteJump && !isGrounded) && (Time.time < timeSinceLeftGround + stats.coyoteTime)) && isGrounded)
        {
            velocity.y = stats.jumpPower;
            rb.velocity = new Vector2(rb.velocity.x, velocity.y);
            canJump = false;
            canCoyoteJump = false;
            jumpCut = true;
            OnJump?.Invoke();
        }
        else if (!Input.GetButton("Jump") &&
            rb.velocity.y>0 && jumpCut)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * stats.jumpMultiplier);
        }
    }

    IEnumerator DisableWallJump(float duration)
    {
        isTouchingWall = false;
        yield return new WaitForSeconds(duration);
    }

    //---------------

    void CheckWallStatus()
    {
        isTouchingWall = Physics2D.CapsuleCast(cCollider.bounds.center,
            cCollider.size,cCollider.direction,0,Vector2.right*Mathf.Sign(inputAxis.x),0.2f,stats.wallLayer);              
    }

    void ProcessDash()
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                rb.velocity = new Vector2(stats.dashSpeed * inputAxis.x, rb.velocity.y);
                dashTimeLeft -= Time.deltaTime;
            }
            else
            {
                isDashing = false;
                rb.velocity = velocity;
            }
        }
    }

    void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = stats.dashDuration;
        lastDash = Time.time;
        rb.velocity = 
            new Vector2(stats.dashSpeed*Mathf.Sign(inputAxis.x),0);
    }

    void ApplyDash()
    {
        if (dashTimeLeft > 0)
        {
            rb.velocity = new Vector2(stats.dashSpeed * Mathf.Sign(inputAxis.x),0);
            dashTimeLeft -= Time.deltaTime;
        }
        else
            isDashing = false;
    }


    //----------
    [Serializable]
    public class PlayerStats {
        public float maxSpeed = 10.0f;
        public float jumpPower = 15.0f;
        public float acceleration = 30f;
        public float groundDeceleration = 20f;
        public float airDeceleration = 5f;
        public float fallAccleration = 30f;
        public float inAirAcceleration = 15f;
        public float MaxFallSpeed = 20f;
        public float GroundForce = -0.5f;   
        public float GroundDistance = 0.1f;
        public float coyoteTime = 0.2f;
        public float jumpBuffer = 0.1f;
        public float jumpMultiplier = 0.5f;
        public LayerMask groundLayer;
        public float HorizontalTreshold = 0.1f;
        public float VerticalTreshold = 0.1f;

        //Double jump
        public int maxJumpCount = 2;
        //Wall jump
        public LayerMask wallLayer;
        public float wallJumpPower = 15.0f;
        public float wallJumpForce = 10.0f;
        public float wallCheckDistance = 0.1f;
        public float tempDisableDuration = 0.2f;
        //Dash 
        public float dashSpeed = 30.0f;
        public float dashDuration = 0.3f;
        public float dashCoolDown = 0.8f;
       
    }
}
