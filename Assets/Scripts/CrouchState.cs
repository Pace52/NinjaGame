using UnityEngine;

public class CrouchState : PlayerBaseState
{
    private float enterTime;
    private float crouchMoveSpeed;
    private Vector2 moveInput;
    private bool wasMovingOnEnter;

    public CrouchState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        // Calculate actual crouch speed based on multipliers
        // Walk speed is 0.5 * MoveSpeed, Crouch is half of Walk speed (0.25 * MoveSpeed)
        crouchMoveSpeed = stateMachine.MoveSpeed * stateMachine.CrouchSpeedMultiplier;
    }

    public override void Enter()
    {
        enterTime = Time.time;
        moveInput = stateMachine.InputReader.GetMovementInput();
        wasMovingOnEnter = moveInput.magnitude > 0.1f;

        // Set to crouching collider
        stateMachine.SetColliderCrouching();

        // Play crouch animation
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetBool("IsCrouching", true);
            if (wasMovingOnEnter)
            {
                stateMachine.Animator.SetBool("IsMoving", true);
            }
        }

        Debug.Log($"[CrouchState] Entering Crouch State at {enterTime:F2}s");
    }

    public override void Tick(float deltaTime)
    {
        // Check for ground contact loss
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

        // Check for Shoot input
        if (stateMachine.InputReader.IsShootPressed())
        {
            stateMachine.SwitchState(stateMachine.ShootState);
            return;
        }

        // Get current movement input
        moveInput = stateMachine.InputReader.GetMovementInput();

        // Handle movement while crouching
        if (stateMachine.RB != null && moveInput != Vector2.zero)
        {
            // Apply crouch movement
            Vector2 targetVelocity = new Vector2(moveInput.x * crouchMoveSpeed, stateMachine.RB.linearVelocity.y);
            stateMachine.RB.linearVelocity = targetVelocity;

            // Update animation
            if (stateMachine.Animator != null)
            {
                stateMachine.Animator.SetBool("IsMoving", true);
            }
        }
        else if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetBool("IsMoving", false);
        }

        // Check for slide initiation
        if (moveInput.magnitude > 0.1f && stateMachine.InputReader.IsRunPressed())
        {
            stateMachine.SwitchState(stateMachine.SlideState);
            return;
        }

        // Check for uncrouch
        if (!stateMachine.InputReader.IsCrouchHeld())
        {
            if (stateMachine.CanStandUp())
            {
                // Determine next state based on input
                if (moveInput.magnitude > 0.1f)
                {
                    if (stateMachine.InputReader.IsRunPressed())
                    {
                        stateMachine.SwitchState(stateMachine.RunState);
                    }
                    else
                    {
                        stateMachine.SwitchState(stateMachine.WalkState);
                    }
                }
                else
                {
                    stateMachine.SwitchState(stateMachine.IdleState);
                }
            }
            else
            {
                Debug.Log("[CrouchState] Cannot stand up, obstacle detected.");
            }
        }
    }

    public override void Exit()
    {
        // Reset crouch animation
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetBool("IsCrouching", false);
            stateMachine.Animator.SetBool("IsMoving", false);
        }

        // Only restore standing collider if we can stand up and we're not going to slide
        if (stateMachine.CanStandUp() && !stateMachine.InputReader.IsCrouchHeld())
        {
            stateMachine.SetColliderStanding();
        }

        Debug.Log($"[CrouchState] Exiting Crouch State after {Time.time - enterTime:F2}s");
    }
}