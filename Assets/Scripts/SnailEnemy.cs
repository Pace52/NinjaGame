using UnityEngine;
using System.Collections;

public class SnailEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackKnockback = 5f;
    private int currentHealth;
    private float lastAttackTime;
    private bool canAttack = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool movingRight = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Determine direction to player
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        movingRight = directionToPlayer > 0;

        // Update sprite direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !movingRight;
        }

        // Check if player is in attack range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && canAttack)
        {
            Attack();
        }
        // Move toward player if not in attack range
        else if (CanMoveInDirection(movingRight))
        {
            float direction = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;
        
        // Visual feedback for attack
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        // Check if player is still in range and in front of us
        if (IsPlayerInAttackRange())
        {
            // Apply damage and knockback to player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                
                // Apply knockback
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDirection = (player.position - transform.position).normalized;
                    playerRb.linearVelocity = knockbackDirection * attackKnockback;
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        // Reset visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        canAttack = true;
    }

    private bool IsPlayerInAttackRange()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange) return false;

        // Check if player is in front of us
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float dotProduct = Vector2.Dot(directionToPlayer, movingRight ? Vector2.right : Vector2.left);
        return dotProduct > 0.5f; // Player must be mostly in front
    }

    private bool CanMoveInDirection(bool isRight)
    {
        Vector2 rayStart = transform.position;
        Vector2 rayDirection = isRight ? Vector2.right : Vector2.left;

        // Wall check
        RaycastHit2D wallHit = Physics2D.Raycast(rayStart, rayDirection, wallCheckDistance, groundLayer);
        if (wallHit.collider != null)
        {
            return false;
        }

        // Ground check (check if there's ground ahead)
        Vector2 groundRayStart = rayStart + (rayDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(groundRayStart, Vector2.down, groundCheckDistance, groundLayer);
        if (groundHit.collider == null)
        {
            return false;
        }

        return true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle player collision - you can add damage logic here
            Debug.Log("Player hit by snail!");
        }
    }

    public void TakeDamage()
    {
        currentHealth--;
        
        // Flash effect
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashEffect()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        // Add death effects here (particles, sound, etc.)
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Draw ground check
        Gizmos.color = Color.green;
        Vector2 groundRayStart = transform.position + (movingRight ? Vector3.right : Vector3.left) * 0.5f;
        Gizmos.DrawLine(groundRayStart, groundRayStart + Vector2.down * groundCheckDistance);

        // Draw wall check
        Gizmos.color = Color.red;
        Vector2 wallRayStart = transform.position;
        Vector2 wallRayDirection = movingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(wallRayStart, wallRayStart + wallRayDirection * wallCheckDistance);

        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
} 