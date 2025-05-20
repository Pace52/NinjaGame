using UnityEngine;

public class WallClingState : PlayerBaseState
{
    private float slideSpeed = 1.5f;
    private float enterTime;
    private bool jumpHeldOnEnter;
    private Vector2 lastWallNormal;
    private float detachCooldown = 0.2f;
    private float lastDetachTime;
    private float minSlideTime = 0.1f;

    public WallClingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        enterTime = Time.time;
        jumpHeldOnEnter = stateMachine.InputReader.IsJumpPressed();

        // Get wall normal (opposite of movement direction)
        Vector2 moveInput = stateMachine.InputReader.GetMovementInput();
        lastWallNormal = moveInput.x != 0 ? new Vector2(-Mathf.Sign(moveInput.x), 0) : 
                        (stateMachine.transform.localScale.x > 0 ? Vector2.left : Vector2.right);

        // Reduce vertical velocity gradually
        if (stateMachine.RB != null)
        {
            float currentVelocity = stateMachine.RB.linearVelocity.y;
            stateMachine.RB.linearVelocity = new Vector2(0, Mathf.Lerp(currentVelocity, -slideSpeed, 0.5f));
        }

        // Set animation
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetBool("IsWallClinging", true);
        }

        Debug.Log($"[WallClingState] Entering Wall Cling State at {enterTime:F2}s");
    }

    public override void Tick(float deltaTime)
    {
        // Check for detachment conditions
        Vector2 moveInput = stateMachine.InputReader.GetMovementInput();
        
        // Prevent immediate reattachment after detaching
        if (Time.time - lastDetachTime < detachCooldown)
        {
            stateMachine.SwitchState(stateMachine.FallState);
            return;
        }

        // Ensure minimum time in wall cling state
        if (Time.time - enterTime < minSlideTime)
        {
            return;
        }
        
        // Detach if pushing away from wall
        if (moveInput.x != 0 && Mathf.Sign(moveInput.x) == Mathf.Sign(lastWallNormal.x))
        {
            DetachFromWall();
            return;
        }

        // Check for ground contact
        if (stateMachine.IsGrounded())
        {
            stateMachine.SwitchState(stateMachine.IdleState);
            return;
        }

        // Check for wall contact loss
        if (!stateMachine.IsTouchingWall())
        {
            stateMachine.SwitchState(stateMachine.FallState);
            return;
        }

        // Handle wall jump
        if (stateMachine.InputReader.IsJumpPressed() && !jumpHeldOnEnter)
        {
            PerformWallJump();
            return;
        }

        // Update jump hold check
        if (!stateMachine.InputReader.IsJumpPressed())
        {
            jumpHeldOnEnter = false;
        }

        // Apply wall slide with smooth deceleration
        if (stateMachine.RB != null)
        {
            float targetVelocity = -slideSpeed;
            float currentVelocity = stateMachine.RB.linearVelocity.y;
            float newVelocity = Mathf.Lerp(currentVelocity, targetVelocity, deltaTime * 5f);
            stateMachine.RB.linearVelocity = new Vector2(0, newVelocity);
        }
    }

    private void PerformWallJump()
    {
        if (stateMachine.RB != null)
        {
            // Calculate wall jump direction
            Vector2 jumpDirection = new Vector2(lastWallNormal.x, 1).normalized;
            
            // Apply wall jump force with a slight upward bias
            stateMachine.RB.linearVelocity = Vector2.zero; // Clear current velocity
            stateMachine.RB.AddForce(jumpDirection * stateMachine.WallJumpForce, ForceMode2D.Impulse);
            
            // Detach from wall
            DetachFromWall();
            
            // Switch to jump state
            stateMachine.SwitchState(stateMachine.JumpState);
        }
    }

    private void DetachFromWall()
    {
        lastDetachTime = Time.time;
        stateMachine.DetachFromWall();
        stateMachine.SwitchState(stateMachine.FallState);
    }

    public override void Exit()
    {
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetBool("IsWallClinging", false);
        }

        Debug.Log($"[WallClingState] Exiting Wall Cling State after {Time.time - enterTime:F2}s");
    }
}