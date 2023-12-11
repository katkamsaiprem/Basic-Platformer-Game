using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D)),RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    // Player Stats object containing configurable parameters
    public PlayerStats stats;

    // Rigidbody and Collider components for physics interactions
    private Rigidbody2D rb;
    private CapsuleCollider2D cCollider;

    // Input and velocity vectors
    private Vector2 inputAxis;
    private Vector2 velocity;

    // State variables for ground check and jump mechanics
    [SerializeField] private bool isGrounded;
    private float timeSinceJumpPressed;
    private float timeSinceLeftGround;
    private bool canJump;
    private bool canCoyoteJump;
    private bool jumpCut;

    // Variables for jump count and wall interactions
    private int jumpCount;
    [SerializeField] private bool isTouchingWall;

    // Variables for dash mechanics
    [SerializeField] private bool isDashing;
    private float dashTimeLeft;
    private float lastDash = -50.0f;

    // Variables for wall sliding
    [SerializeField] private bool isWallSliding;
   
    // Events for ground change and jump actions
    public event Action<bool> OnGroundedChange;
    public event Action OnJump;

    private void Awake()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        cCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        // Process player input each frame
        ProcessInput();
        ProcessDash();
    }

    private void FixedUpdate()
    {
        // Check and apply physics-based mechanics each physics update
        CheckWallStatus();
        CheckGroundStatus();
        CheckWallSlide();
        ApplyJump();
        ApplyMovement();
        ApplyGravity();

        // Apply wall sliding if the player is sliding
        if (isWallSliding)
        {
            ApplyWallSlide();
        }

        // Apply dashing if the player is dashing
        if (isDashing)
        {
            ApplyDash();
        }
    }

    void ProcessInput()
    {
        // Read horizontal and vertical input
        inputAxis.x = Input.GetAxisRaw("Horizontal");
        inputAxis.y = Input.GetAxisRaw("Vertical");

        // Check if the jump button is pressed
        if (Input.GetButtonDown("Jump"))
        {
            canJump = true;
            timeSinceJumpPressed = Time.time;
        }

        // Check if the dash button is pressed and if dash cooldown is finished
        if (Input.GetButtonDown("Dash") && Time.time >= (lastDash + stats.dashCoolDown))
        {
            AttemptToDash();
        }
    }

    void CheckGroundStatus()
    {
        // Check if the player is grounded using a capsule cast
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.CapsuleCast(cCollider.bounds.center, cCollider.size, cCollider.direction, 0, Vector2.down, stats.GroundDistance, stats.groundLayer);

        // Reset jump count if grounded
        if (isGrounded)
        {
            jumpCount = 0;
        }

        // Trigger ground change events if the grounded state changes
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
        // Apply horizontal movement based on input
        float targetSpeed = inputAxis.x * stats.maxSpeed;
        rb.velocity = new Vector2(targetSpeed, rb.velocity.y);
    }

    void ApplyJump()
    {
        // Regular Jump Logic
        if (canJump && (isGrounded || jumpCount < stats.maxJumpCount))
        {
            // If the player can jump and is either grounded or hasn't reached the max jump count...
            // Set the vertical velocity to the jump power defined in stats
            velocity.y = stats.jumpPower;
            rb.velocity = new Vector2(rb.velocity.x, velocity.y);

            // Increment the jump count as the player has performed a jump
            jumpCount++;

            // Set canJump to false to prevent repeated jumps without new input
            canJump = false;

            // Invoke the OnJump event which can be used for animations or sound effects
            OnJump?.Invoke();
        }
        // Wall Jump Logic
        else if (isWallSliding && canJump)
        {
            // If the player is sliding on a wall and can jump...
            // Reset the Y velocity to ensure the player jumps upwards
            rb.velocity = new Vector2(rb.velocity.x, 0);

            // Calculate the direction of the wall jump (away from the wall and upwards)
            Vector2 wallJumpDirection = new Vector2(-Mathf.Sign(inputAxis.x), transform.position.y + 10).normalized;

            // Apply a force in the calculated direction using the wall jump power
            rb.AddForce(wallJumpDirection * stats.wallJumpPower, ForceMode2D.Impulse);

            // Do not increment jumpCount to allow for continuous wall jumping

            // Reset canJump to false and stop wall sliding
            canJump = false;
            isWallSliding = false;

            // Invoke the OnJump event for wall jump
            OnJump?.Invoke();
        }

        // Coyote Time Jump Logic
        else if ((canJump || (canCoyoteJump && !isGrounded) && (Time.time < timeSinceLeftGround + stats.coyoteTime)) && isGrounded)
        {
            // Coyote time allows the player to jump shortly after leaving a ledge
            // If within the coyote time window, set the vertical velocity to jump power
            velocity.y = stats.jumpPower;
            rb.velocity = new Vector2(rb.velocity.x, velocity.y);

            // Reset jump variables
            canJump = false;
            canCoyoteJump = false;
            jumpCut = true;

            // Invoke the OnJump event
            OnJump?.Invoke();
        }

        // Jump Cut-Off Logic
        else if (!Input.GetButton("Jump") && rb.velocity.y > 0 && jumpCut)
        {
            // If the jump button is released while going upwards, reduce the jump height
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * stats.jumpMultiplier);
        }
    }

    void CheckWallSlide()
    {
        // Check if the player is sliding on a wall
        isWallSliding = isTouchingWall && !isGrounded && rb.velocity.y < 0;
    }

    void ApplyWallSlide()
    {
        // Apply wall sliding mechanics
        if (isWallSliding)
        {
            if (rb.velocity.y < -stats.wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -stats.wallSlideSpeed);
            }
        }
    }

    void ApplyGravity()
    {
        // Check if the player is not grounded
        if (!isGrounded)
        {
            // Determine the gravity force to apply
            // If the player is moving upwards and the jump button is not being held,
            // apply a higher gravity (fallAcceleration) for a 'weightier' feel when falling.
            // Otherwise, apply normal in-air gravity (inAirAcceleration)
            float gravityForce = (rb.velocity.y > 0 && !Input.GetButton("Jump")) ? stats.fallAccleration : stats.inAirAcceleration;

            // Gradually move the vertical velocity towards the maximum fall speed
            // This creates a smooth transition in velocity, adding realism to the jump and fall
            velocity.y = Mathf.MoveTowards(rb.velocity.y, -stats.MaxFallSpeed, gravityForce * Time.fixedDeltaTime);

            // Apply the calculated velocity to the Rigidbody2D component
            // This modifies only the y-component (vertical) of the velocity, preserving horizontal movement
            rb.velocity = new Vector2(rb.velocity.x, velocity.y);
        }
    }


    void CheckWallStatus()
    {
        // Check if the player is touching a wall using a capsule cast
        isTouchingWall = Physics2D.CapsuleCast(cCollider.bounds.center, cCollider.size, cCollider.direction, 0, Vector2.right * Mathf.Sign(inputAxis.x), 0.2f, stats.wallLayer);
    }

    void ProcessDash()
    {
        // Handle dashing mechanics
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
        // Initiate a dash
        isDashing = true;
        dashTimeLeft = stats.dashDuration;
        lastDash = Time.time;
        rb.velocity = new Vector2(stats.dashSpeed * Mathf.Sign(inputAxis.x), 0);
    }

    void ApplyDash()
    {
        // Apply dashing movement
        if (dashTimeLeft > 0)
        {
            rb.velocity = new Vector2(stats.dashSpeed * Mathf.Sign(inputAxis.x), 0);
            dashTimeLeft -= Time.deltaTime;
        }
        else
        {
            isDashing = false;
        }
    }

    //IEnumerator DisableWallJump(float duration)
    //{
    //    // Coroutine to temporarily disable wall jumping
    //    isTouchingWall = false;
    //    yield return new WaitForSeconds(duration);
    //}


    // Player parameters and properties
    // Include fields like maxSpeed, jumpPower, dashSpeed, etc.
    [Serializable]
    public class PlayerStats {

        // Maximum horizontal speed the player can achieve
        public float maxSpeed = 10.0f;

        // The force applied when the player jumps
        public float jumpPower = 15.0f;

        // Acceleration while moving on the ground
        public float acceleration = 30f;

        // Deceleration when stopping or changing directions on the ground
        public float groundDeceleration = 20f;

        // Deceleration when stopping or changing directions in the air
        public float airDeceleration = 5f;

        // Acceleration applied when falling (to make fall faster than jump)
        public float fallAccleration = 30f;

        // Acceleration applied when moving in the air
        public float inAirAcceleration = 15f;

        // Maximum speed the player can reach while falling
        public float MaxFallSpeed = 20f;

        // Force applied downwards when the player is grounded
        public float GroundForce = -0.5f;

        // Distance from the player's feet to the ground to check if grounded
        public float GroundDistance = 0.1f;

        // Time after leaving a ledge during which the player can still jump
        public float coyoteTime = 0.2f;

        // Time before hitting the ground that the player can press jump and still jump
        public float jumpBuffer = 0.1f;

        // Multiplier for jump height when the jump button is released quickly
        public float jumpMultiplier = 0.5f;

        // LayerMask to determine what constitutes the ground
        public LayerMask groundLayer;

        // Thresholds for horizontal and vertical input sensitivity
        public float HorizontalTreshold = 0.1f;
        public float VerticalTreshold = 0.1f;

        // Maximum number of jumps the player can perform without touching the ground
        public int maxJumpCount = 2;

        // LayerMask to determine what constitutes a wall for wall jumping
        public LayerMask wallLayer;

        // Force applied for wall jumps
        public float wallJumpPower = 15.0f;

        // Additional force applied horizontally during a wall jump
        public float wallJumpForce = 10.0f;

        // Distance from the player's side to check for a wall
        public float wallCheckDistance = 0.1f;

        // Duration for which wall jumping is disabled after a wall jump
        public float tempDisableDuration = 0.2f;

        // Speed at which the player moves during a dash
        public float dashSpeed = 30.0f;

        // Duration of the dash movement
        public float dashDuration = 0.3f;

        // Cooldown time between dashes
        public float dashCoolDown = 0.8f;

        //Wall Slide speed
        public float wallSlideSpeed = 3f; // Configurable wall slide speed


       
    }
}
