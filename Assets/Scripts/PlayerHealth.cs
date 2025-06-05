using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 3;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"Player took damage! Health: {health}");
        if (health <= 0)
        {
            Debug.Log("Player health <= 0. Calling Die().");
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Die() called. Resetting scene.");
        // Try to use PlayerStateMachine's Die if available
        var stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.SendMessage("Die");
        }
        else
        {
            // Fallback: reload the current scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
} 