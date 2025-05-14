using UnityEngine;
using System.Collections.Generic;

// Base class for all player states
public abstract class PlayerBaseState
{
    protected PlayerStateMachine stateMachine;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public abstract void Enter();
    public abstract void Tick(float deltaTime);
    public abstract void Exit();
}

// The main state machine component
public class PlayerStateMachine : MonoBehaviour
{
    public int coinCount = 0;
    private bool wasGroundedLastFrame = true;

    // --- Coyote time (grounded grace period) ---
    //private CapsuleCollider2D playerCollider;
    private float jumpGroundedGraceTimer = 0f;
    private const float jumpGroundedGraceDuration = 0.10f; // 0.1 seconds of grace after jumping
    [field: SerializeField] public float MoveSpeed { get; private set; } = 5f; // Example speed
    [field: SerializeField] public float WallJumpForce { get; private set; } = 7.5f;
    [field: SerializeField] public int MaxJumps { get; private set; } = 2; // 1 = no double jump, 2 = double jump
    public int JumpsRemaining { get; set; }

    [Header("Collider Settings")]
    public Collider2D collider;
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private Vector2 standingColliderSize = new Vector2(0.5f, 1.8f); // Adjusted for better proportions
    [SerializeField] private Vector2 standingColliderOffset = new Vector2(0f, 0.9f); // Center point is at feet
    [SerializeField] private Vector2 crouchingColliderSize = new Vector2(0.5f, 0.9f); // Half height when crouching
    [SerializeField] private Vector2 crouchingColliderOffset = new Vector2(0f, 0.45f); // Adjusted to keep feet position
    [SerializeField] private float standUpCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.1f;

    [Header("Crouch Settings")]
    [SerializeField] public float CrouchSpeedMultiplier { get; private set; } = 0.25f; // Half of WalkState's 0.5 multiplier


    private PlayerBaseState currentState;

    // State registry for extensibility
    private Dictionary<string, PlayerBaseState> stateRegistry = new Dictionary<string, PlayerBaseState>();

    // Concrete states
    public PlayerIdleState IdleState { get; private set; } // Add IdleState property back

    public WalkState WalkState { get; private set; }
    public RunState RunState { get; private set; }
    public JumpState JumpState { get; private set; }
    public CrouchState CrouchState { get; private set; }
    public SlideState SlideState { get; private set; }
    public WallClingState WallClingState { get; private set; }
    public ShootState ShootState { get; private set; } // Add ShootState declaration
    public FallState FallState { get; private set; } // Add FallState declaration
    // Component References (Example)
    public Rigidbody2D RB { get; private set; }
    public Animator Animator { get; private set; }
    // Add InputReader reference if using one

    // State transition event
    public delegate void StateChangedEvent(PlayerBaseState fromState, PlayerBaseState toState);
    public event StateChangedEvent OnStateChanged;

    // State duration tracking
    private float stateEnterTime;
    public float GetStateDuration()
    {
        return Time.time - stateEnterTime;
    }
    /*public ColliderHeight()
    {
        return colliderHeight;
    }*/

    // InputReader abstraction (now a separate class)
    public InputReader InputReader { get; private set; } // Public property for states to access

    private void Awake()
    {
        // Get Components
        RB = GetComponent<Rigidbody2D>();
        Animator = GetComponentInChildren<Animator>();
        
        // Setup collider if not assigned
        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider2D>();
        }

        if (playerCollider != null)
        {
            // Initialize collider with standing size
            playerCollider.size = standingColliderSize;
            playerCollider.offset = standingColliderOffset;
        }
        else
        {
            Debug.LogError("Player Collider not found or assigned!", this);
        }

        // Setup ground check point if not assigned
        if (groundCheckPoint == null)
        {
            // Create ground check point
            GameObject checkPoint = new GameObject("GroundCheckPoint");
            groundCheckPoint = checkPoint.transform;
            groundCheckPoint.parent = transform;
            groundCheckPoint.localPosition = new Vector3(0, -0.1f, 0); // Slightly below feet
        }

        // Initialize input reader
        InputReader = new InputReader();

        // Initialize states
        InitializeStates();
    }

    private void InitializeStates()
    {
        // Initialize concrete states
        IdleState = new PlayerIdleState(this);
        WalkState = new WalkState(this);
        RunState = new RunState(this);
        JumpState = new JumpState(this);
        CrouchState = new CrouchState(this);
        SlideState = new SlideState(this);
        WallClingState = new WallClingState(this);
        ShootState = new ShootState(this);
        FallState = new FallState(this);

        // Register states
        stateRegistry.Clear();
        stateRegistry[nameof(PlayerIdleState)] = IdleState;
        stateRegistry[nameof(WalkState)] = WalkState;
        stateRegistry[nameof(RunState)] = RunState;
        stateRegistry[nameof(JumpState)] = JumpState;
        stateRegistry[nameof(CrouchState)] = CrouchState;
        stateRegistry[nameof(SlideState)] = SlideState;
        stateRegistry[nameof(WallClingState)] = WallClingState;
        stateRegistry[nameof(ShootState)] = ShootState;
        stateRegistry[nameof(FallState)] = FallState;

        // Initialize jumps
        JumpsRemaining = MaxJumps;
    }

    private void Start()
    {
        // Set the initial state
        SwitchState(IdleState); // Start in Idle state
        JumpsRemaining = MaxJumps;
    }
    public CapsuleCollider2D GetPlayerCollider()
    {
        return playerCollider;
    }
    private void Update()
    {
        // Update coyote time timer
        if (jumpGroundedGraceTimer > 0f)
            jumpGroundedGraceTimer -= Time.deltaTime;

        // Track grounded state for jump reset logic
        bool isGroundedNow = IsGrounded();
        if (!wasGroundedLastFrame && isGroundedNow)
        {
            // Landed this frame, reset jumps
            JumpsRemaining = Mathf.Max(0, MaxJumps - 1);
        }
        wasGroundedLastFrame = isGroundedNow;

        currentState?.Tick(Time.deltaTime);
    }

    public void SwitchState(PlayerBaseState newState)
    {
        if (currentState == newState) return; // Re-entrancy guard
        var prevState = currentState;
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
        OnStateChanged?.Invoke(prevState, newState);
        stateEnterTime = Time.time;
    }

    public Vector2 GetMovementInput()
    {
        // Delegate to the InputReader instance
        return InputReader.GetMovementInput();
    }

    public bool IsRunPressed()
    {
        // Delegate to the InputReader instance
        return InputReader.IsRunPressed();
    }

    // For extensibility: get state by name
    public PlayerBaseState GetState(string stateName)
    {
        if (stateRegistry.TryGetValue(stateName, out var state))
            return state;
        return null;
    }
    // Robust ground check using OverlapCircle
    public bool IsGrounded()
    {
        if (jumpGroundedGraceTimer > 0f)
            return false;

        if (groundCheckPoint == null)
        {
            Debug.LogError("Ground Check Point not assigned!", this);
            return false;
        }

        // Perform the ground check
        bool grounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        
        // Reset jumps when grounded
        if (grounded && !wasGroundedLastFrame)
        {
            JumpsRemaining = MaxJumps;
        }

        wasGroundedLastFrame = grounded;
        return grounded;
    }

    // Simple wall check (replace with your own logic)
    public bool IsTouchingWall()
    {
        // Wall detection using 2D raycast
        float wallCheckDistance = 0.1f; // How far to check
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left; // Check based on facing direction
        RaycastHit2D hit = Physics2D.Raycast(playerCollider.bounds.center, direction, playerCollider.bounds.extents.x + wallCheckDistance, groundLayer);
        Debug.DrawRay(playerCollider.bounds.center, direction * (playerCollider.bounds.extents.x + wallCheckDistance), hit.collider != null ? Color.green : Color.red);
        return hit.collider != null;
    }

    public void SetColliderCrouching()
    {
        if (playerCollider == null) return;
        
        // Store current y position of feet
        float feetY = transform.position.y - playerCollider.offset.y + (playerCollider.size.y / 2f);
        
        // Set new size and offset
        playerCollider.size = crouchingColliderSize;
        playerCollider.offset = crouchingColliderOffset;
        
        // Adjust position to maintain feet position
        float newFeetY = transform.position.y - playerCollider.offset.y + (playerCollider.size.y / 2f);
        float adjustment = feetY - newFeetY;
        transform.position = new Vector3(transform.position.x, transform.position.y + adjustment, transform.position.z);
    }

    public void SetColliderStanding()
    {
        if (playerCollider == null) return;
        
        // Only change if we can stand up
        if (CanStandUp())
        {
            // Store current y position of feet
            float feetY = transform.position.y - playerCollider.offset.y + (playerCollider.size.y / 2f);
            
            // Set new size and offset
            playerCollider.size = standingColliderSize;
            playerCollider.offset = standingColliderOffset;
            
            // Adjust position to maintain feet position
            float newFeetY = transform.position.y - playerCollider.offset.y + (playerCollider.size.y / 2f);
            float adjustment = feetY - newFeetY;
            transform.position = new Vector3(transform.position.x, transform.position.y + adjustment, transform.position.z);
        }
    }

    public bool CanStandUp()
    {
        if (playerCollider == null) return true;

        // Calculate the position where the standing collider would be
        Vector2 standingCenter = (Vector2)transform.position + standingColliderOffset;
        
        // Check for obstacles using a box cast
        float extraHeight = standingColliderSize.y - crouchingColliderSize.y;
        Vector2 boxSize = new Vector2(standingColliderSize.x * 0.9f, extraHeight);
        Vector2 boxCenter = standingCenter + Vector2.up * (crouchingColliderSize.y / 2f);
        
        // Perform the check
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.up,
            standUpCheckDistance,
            groundLayer
        );

        return !hit.collider;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is a coin
        if (other.CompareTag("Coin"))
        {
            // Remove the coin from the scene
            Destroy(other.gameObject);

            // Increment the coin count by one
            coinCount++;

            // Optionally, print the new coin count for debugging
            Debug.Log("Coins collected: " + coinCount);
        }
    } 
}