using UnityEngine;

public class SlideState : PlayerBaseState
{
    private float slideStartTime;
    private float slideDuration = 0.75f; // Reduced duration for better feel
    private Vector2 slideDirection;
    private float slideSpeed = 8f; // Base slide speed
    private float slideSpeedDecay = 0.8f; // How quickly slide speed decreases

    public SlideState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        slideStartTime = Time.time;
        slideDirection = stateMachine.InputReader.GetMovementInput().normalized;
        if (slideDirection == Vector2.zero)
        {
            slideDirection = stateMachine.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }

        // Set collider to crouching size
        stateMachine.SetColliderCrouching();

        // Play slide animation
        if (stateMachine.Animator != null)
            stateMachine.Animator.SetBool("IsSliding", true);

        // Apply initial slide velocity
        if (stateMachine.RB != null)
        {
            float initialSlideSpeed = slideSpeed * (stateMachine.RB.linearVelocity.magnitude / stateMachine.MoveSpeed);
            stateMachine.RB.linearVelocity = new Vector2(slideDirection.x * initialSlideSpeed, stateMachine.RB.linearVelocity.y);
        }

        Debug.Log($"[SlideState] Entering Slide State at {slideStartTime:F2}s");
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

        float timeSinceSlideStarted = Time.time - slideStartTime;
        float slideSpeedMultiplier = Mathf.Pow(slideSpeedDecay, timeSinceSlideStarted);

        // Apply sliding physics with decay
        if (stateMachine.RB != null)
        {
            float currentSlideSpeed = slideSpeed * slideSpeedMultiplier;
            stateMachine.RB.linearVelocity = new Vector2(slideDirection.x * currentSlideSpeed, stateMachine.RB.linearVelocity.y);
        }

        // Check for slide end conditions
        if (timeSinceSlideStarted >= slideDuration || currentSlideSpeed < stateMachine.MoveSpeed * 0.5f)
        {
            if (stateMachine.InputReader.IsCrouchHeld())
            {
                stateMachine.SwitchState(stateMachine.CrouchState);
            }
            else if (stateMachine.CanStandUp())
            {
                if (stateMachine.InputReader.IsRunPressed() && stateMachine.InputReader.GetMovementInput().magnitude > 0.1f)
                {
                    stateMachine.SwitchState(stateMachine.RunState);
                }
                else if (stateMachine.InputReader.GetMovementInput().magnitude > 0.1f)
                {
                    stateMachine.SwitchState(stateMachine.WalkState);
                }
                else
                {
                    stateMachine.SwitchState(stateMachine.IdleState);
                }
            }
        }
    }

    public override void Exit()
    {
        // Reset animation
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetBool("IsSliding", false);
        }

        // Only restore standing collider if we're not going to crouch state
        if (!stateMachine.InputReader.IsCrouchHeld() && stateMachine.CanStandUp())
        {
            stateMachine.SetColliderStanding();
        }

        Debug.Log($"[SlideState] Exiting Slide State after {Time.time - slideStartTime:F2}s");
    }
}