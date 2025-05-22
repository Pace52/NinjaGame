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
    [SerializeField] private int currentHealth;
    private float lastAttackTime;
    private bool canAttack = true;
    private float attackCooldownTimer = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool movingRight = true;

    private static int snailCounter = 0;
    private int snailId;

    private void Awake()
    {
        snailId = ++snailCounter;
        Debug.Log($"[Snail {snailId}] Awake. Instance created.");
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        Debug.Log($"[Snail {snailId}] Start. Health: {currentHealth}");
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError($"[Snail {snailId}] Player not found! Make sure the player has the 'Player' tag.");
        }
    }

    private void Update()
    {
        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
        if (player == null) return;
        //Debug.Log($"[Snail {snailId}] Update. Health: {currentHealth}, Position: {transform.position}, Active: {gameObject.activeSelf}");

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
        Debug.Log("AAAA");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collided with player collisions");
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($"Snail collided with Player. Health: {currentHealth}");
                // Determine collision direction
                ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
                collision.GetContacts(contacts);
                foreach (var contact in contacts)
                {
                    Vector2 contactNormal = contact.normal;
                    Debug.Log($"Contact normal: {contactNormal}");
                    // If the contact normal points up, player landed on snail
                    if (contactNormal.y > 0.5f)
                    {
                        Debug.Log("Player landed on snail. Snail will die.");
                        Die();
                        // Optionally bounce the player up
                        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                        if (playerRb != null) playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);
                        return;
                    }
                    // If the contact normal points left/right, check if it's the front
                    if (attackCooldownTimer <= 0f && ((movingRight && contactNormal.x < -0.5f) || (!movingRight && contactNormal.x > 0.5f)))
                    {
                        Debug.Log("Player hit by snail's front. Player will take damage.");
                        TakeDamage();
                        attackCooldownTimer = 0.5f;
                        // Optionally knock back the player
                        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                        if (playerRb != null)
                        {
                            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                            playerRb.linearVelocity = new Vector2(knockbackDir.x * attackKnockback, 4f);
                        }
                        return;
                    }
                }
            }
        }
    }

    public void TakeDamage()
    {
        Debug.Log($"[Snail {snailId}] TakeDamage() called. Current health: {currentHealth}");
        currentHealth--;
        Debug.Log($"[Snail {snailId}] Snail took damage! New health: {currentHealth}");
        // Flash effect
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }
        if (currentHealth <= 0)
        {
            Debug.Log($"[Snail {snailId}] Health <= 0. Calling Die().");
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
        Debug.Log($"[Snail {snailId}] Die() called. Destroying snail GameObject. Health: {currentHealth}");
        // Add death effects here (particles, sound, etc.)
        Destroy(gameObject);
        Debug.Log($"[Snail {snailId}] Destroy(gameObject) called. (If you see this log again, object is still alive.)");
    }

    private void OnDestroy()
    {
        Debug.Log($"[Snail {snailId}] OnDestroy called. Snail should now be destroyed.");
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