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
        Debug.Log("Player Die() called. Destroying player GameObject.");
        // Add player death logic here (e.g., play animation, disable controls, respawn, etc.)
        Destroy(gameObject);
        Debug.Log("Player GameObject should now be destroyed.");
    }
} 