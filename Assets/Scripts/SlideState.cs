using UnityEngine;

public class SlideState : PlayerBaseState
{
    private float slideStartTime;
    private float slideDuration = 1.0f; // Example duration, adjust as needed
    private Vector2 slideDirection;
    Collider collider;
    //float OriginalColliderHeight;
    //float colliderheight;
    public SlideState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        if (stateMachine.GetPlayerCollider() != null)
    {
        //stateMachine.GetPlayerCollider().size = stateMachine.standingColliderSize;
        //stateMachine.GetPlayerCollider().offset = stateMachine.standingColliderOffset;
    }
        slideStartTime = Time.time;
        slideDirection = stateMachine.InputReader.GetMovementInput().normalized; // Use InputReader property
        if (slideDirection == Vector2.zero)
        {
            // If no input, slide in the direction the player was last moving, or default forward
            // This needs refinement based on how movement direction is tracked
            slideDirection = stateMachine.transform.forward; // Placeholder
        }

        // Play slide animation
        if (stateMachine.Animator != null)
            stateMachine.Animator.Play("SlideAnimation"); // Ensure this animation exists

        Debug.Log($"[SlideState] Entering Slide State at {slideStartTime:F2}s");
        // Apply initial slide impulse or set velocity
        // stateMachine.RB.velocity = slideDirection * stateMachine.SlideSpeed; // Need SlideSpeed property

        // Adjust collider size/shape for sliding
        // stateMachine.Collider.height = stateMachine.SlideColliderHeight; // Need properties
    }

    public override void Tick(float deltaTime)
    {
        // --- NEW: Check for loss of ground or wall contact ---
        if (!stateMachine.IsGrounded())
        {
            if (stateMachine.IsTouchingWall() && stateMachine.RB.linearVelocity.y <= 0)
            {
                stateMachine.SwitchState(stateMachine.WallClingState);
            }
            else
            {
                stateMachine.SwitchState(stateMachine.FallState);
            }
            return;
        }

        // Check for Shoot input first
        if (stateMachine.InputReader.IsShootPressed()) // Use InputReader property
        {
            stateMachine.SwitchState(stateMachine.ShootState);
            return; // Exit early
        }

        float timeSinceSlideStarted = Time.time - slideStartTime;

        // Apply sliding physics (e.g., decreasing velocity over time)
        float slideFactor = 1 - (timeSinceSlideStarted / slideDuration);
        // stateMachine.RB.velocity = slideDirection * stateMachine.SlideSpeed * slideFactor;

        // Check for slide end condition (duration elapsed, collision, etc.)
        if (timeSinceSlideStarted >= slideDuration)
        {
            // Transition back to a grounded state like Idle or Crouch
            // Check if crouch is still held to decide between CrouchState or Idle/WalkState
            if (stateMachine.InputReader.IsCrouchHeld()) // Use InputReader property
            {
                 stateMachine.SwitchState(stateMachine.CrouchState);
            }
            else
            {
                 stateMachine.SwitchState(stateMachine.IdleState); // Or WalkState if moving
            }
            return;
        }

        // Optional: Allow slight direction control during slide?
        // Vector2 moveInput = stateMachine.GetMovementInput();
        // Apply influence based on moveInput

        // Debug log
        if (Mathf.FloorToInt(timeSinceSlideStarted * 2) % 2 == 0) // Log every half second
        {
             Debug.Log($"[SlideState] Sliding for {timeSinceSlideStarted:F1} seconds");
        }
    }

    /*public override void Exit()
    {
        // Assuming you have a collider component attached to the state machine's GameObject
        //Collider collider = GetComponent<Collider>(); 

        // If OriginalColliderHeight is a float variable stored in your StateMachine class
        //collider.height = OriginalColliderHeight;

        // Or, if you have an accessor method in your StateMachine for the original height:
        //collider.height = StateMachine.OriginalColliderHeight; 

        // Restore collider size/shape
        //stateMachine.GetComponent<Collider>().height = stateMachine.OriginalColliderHeight; // Need properties

        // Ensure velocity is reasonable upon exiting slide
        //stateMachine.RB.linearVelocity *= 0.5f; // Example: reduce speed slightly

        Debug.Log($"[SlideState] Exiting Slide State after {Time.time - slideStartTime:F2}s");
    }
}*/
    public override void Exit()
    {
    // Restore the collider size/offset back to original standing values
    /*if (stateMachine.playerCollider != null)
    {
        stateMachine.playerCollider.size = stateMachine.standingColliderSize;
        stateMachine.playerCollider.offset = stateMachine.standingColliderOffset;
    }*/

    // Reset velocity if necessary (e.g., stop downward motion after slide)
    if (stateMachine.RB != null)
    {
        stateMachine.RB.linearVelocity = new Vector2(stateMachine.RB.linearVelocity.x, 0); // Optional: Reset the Y velocity
    }

    // Stop the sliding animation (if applicable)
    if (stateMachine.Animator != null)
    {
        stateMachine.Animator.SetBool("IsSliding", false); // Ensure this matches your Animator parameter
    }

    // Check for crouch and transition accordingly
    if (stateMachine.InputReader.IsCrouchHeld())
    {
        stateMachine.SwitchState(stateMachine.CrouchState);
    }
    else
    {
        // Transition to idle or walk depending on movement input
        if (stateMachine.InputReader.IsRunPressed())
        {
            stateMachine.SwitchState(stateMachine.RunState);
        }
        else
        {
            stateMachine.SwitchState(stateMachine.IdleState); // Or WalkState if the player is still moving
        }
    }

    Debug.Log($"[SlideState] Exiting Slide State after {Time.time - slideStartTime:F2}s");
    }
}