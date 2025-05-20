using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    // Consider using Unity's new Input System for more robust handling
    // For now, using the legacy Input Manager

    private Vector2 movementInput;
    private bool isRunPressed;
    private bool isJumpPressed;
    private bool isShootPressed;

    public Vector2 GetMovementInput()
    {
        return movementInput;
    }

    public bool IsRunPressed()
    {
        return isRunPressed;
    }

    public bool IsJumpPressed()
    {
        return isJumpPressed;
    }

    public bool IsCrouchHeld()
    {
        // Use GetKey for continuous check while held
        // Consider making the key configurable
        return Input.GetKey(KeyCode.C);
    }

    public bool IsShootPressed()
    {
        return isShootPressed;
    }

    private void Update()
    {
        // Movement input
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        // Run input
        isRunPressed = Input.GetKey(KeyCode.LeftShift);
        
        // Jump input
        isJumpPressed = Input.GetKeyDown(KeyCode.Space);
        
        // Shoot input
        isShootPressed = Input.GetKeyDown(KeyCode.F);
    }
}