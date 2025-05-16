using UnityEngine;

public class WallClingState : PlayerBaseState
{
    private float slideSpeed = 1.5f;
    private float enterTime;
    private bool jumpHeldOnEnter;
    private Vector2 lastWallNormal;

    public WallClingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        enterTime = Time.time;
        jumpHeldOnEnter = stateMachine.InputReader.IsJumpPressed();

        // Get wall normal (opposite of movement direction)
        Vector2 moveInput = stateMachine.InputReader.GetMovementInput();
        lastWallNormal = moveInput.x != 0 ? new Vector2(-Mathf.Sign(moveInput.x), 0) : 
                        (stateMachine.transform.localScale.x > 0 ? Vector2.left : Vector2.right);

        // Reduce vertical velocity
        if (stateMachine.RB != null)
        {
            stateMachine.RB.linearVelocity = new Vector2(0, Mathf.Max(-slideSpeed, stateMachine.RB.linearVelocity.y));
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

        // Apply wall slide
        if (stateMachine.RB != null)
        {
            stateMachine.RB.linearVelocity = new Vector2(0, -slideSpeed);
        }
    }

    private void PerformWallJump()
    {
        if (stateMachine.RB != null)
        {
            // Calculate wall jump direction
            Vector2 jumpDirection = new Vector2(lastWallNormal.x, 1).normalized;
            
            // Apply wall jump force
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