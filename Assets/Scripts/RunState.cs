using UnityEngine;

public class RunState : PlayerBaseState
{
    private float enterTime;

    public RunState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        enterTime = Time.time;
        // Play run animation
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.Play("RunAnimation");
        }
        Debug.Log($"[RunState] Entering Run State at {enterTime:F2}s");
    }

    public override void Tick(float deltaTime)
    {
        // Check for loss of ground
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

        Vector2 moveInput = stateMachine.InputReader.GetMovementInput();

        // Check for Crouch/Slide input if grounded
        if (stateMachine.IsGrounded() && stateMachine.InputReader.IsCrouchHeld())
        {
            if (moveInput.magnitude > 0.1f)
            {
                stateMachine.SwitchState(stateMachine.SlideState);
            }
            else
            {
                stateMachine.SwitchState(stateMachine.CrouchState);
            }
            return;
        }

        // Check for Jump input
        if (stateMachine.InputReader.IsJumpPressed() && stateMachine.JumpsRemaining > 0)
        {
            stateMachine.SwitchState(stateMachine.JumpState);
            return;
        }

        // Handle movement
        if (stateMachine.RB != null)
        {
            float targetVelocityX = moveInput.x * stateMachine.MoveSpeed;
            stateMachine.RB.linearVelocity = new Vector2(targetVelocityX, stateMachine.RB.linearVelocity.y);
        }

        // Transition to walk/idle if run is released or no movement input
        if (!stateMachine.InputReader.IsRunPressed() || moveInput.magnitude < 0.1f)
        {
            if (moveInput.magnitude < 0.1f)
            {
                stateMachine.SwitchState(stateMachine.IdleState);
            }
            else
            {
                stateMachine.SwitchState(stateMachine.WalkState);
            }
        }
    }

    public override void Exit()
    {
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetBool("IsRunning", false);
        }
        Debug.Log($"[RunState] Exiting Run State after {Time.time - enterTime:F2}s");
    }
}