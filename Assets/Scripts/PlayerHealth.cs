using UnityEngine;
using System.Collections;

//private int DeadSnailCount;
//private int coins;
public class PlayerHealth : MonoBehaviour
{
    public int health = 3;
    public static System.Action<int> OnHealthChanged;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"Player took damage! Health: {health}");
        // Flash red when taking damage
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            StartCoroutine(FlashRed(sr));
        OnHealthChanged?.Invoke(health);
        if (health <= 0)
        {
            Debug.Log("Player health <= 0. Calling Die().");
            Die();
            health = 3;
            //coins = 0;
            //DeadSnailCount = 0;
        }
    }

    private System.Collections.IEnumerator FlashRed(SpriteRenderer sr)
    {
        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
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