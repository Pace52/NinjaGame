using UnityEngine;

public class SnailEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.2f;

    [Header("Detection")]
    [SerializeField] private float playerDetectionRange = 5f;
    [SerializeField] private LayerMask playerLayer;

    private Vector2 startPosition;
    private bool movingRight = true;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    private void Update()
    {
        // Check for player in range
        CheckForPlayer();

        // Patrol movement
        Patrol();
    }

    private void Patrol()
    {
        // Calculate movement direction
        float direction = movingRight ? 1f : -1f;
        
        // Move the snail
        rb.velocity = new Vector2(moveSpeed * direction, rb.velocity.y);

        // Update sprite direction
        spriteRenderer.flipX = !movingRight;

        // Check if we need to turn around
        if (ShouldTurnAround())
        {
            movingRight = !movingRight;
        }
    }

    private bool ShouldTurnAround()
    {
        // Check if we've reached patrol distance limit
        float distanceFromStart = transform.position.x - startPosition.x;
        if ((movingRight && distanceFromStart > patrolDistance) ||
            (!movingRight && distanceFromStart < -patrolDistance))
        {
            return true;
        }

        // Check for walls or edges
        Vector2 rayStart = transform.position;
        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;

        // Wall check
        RaycastHit2D wallHit = Physics2D.Raycast(rayStart, rayDirection, wallCheckDistance, groundLayer);
        if (wallHit.collider != null)
        {
            return true;
        }

        // Ground check (check if there's ground ahead)
        Vector2 groundRayStart = rayStart + (rayDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(groundRayStart, Vector2.down, groundCheckDistance, groundLayer);
        if (groundHit.collider == null)
        {
            return true;
        }

        return false;
    }

    private void CheckForPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, playerDetectionRange, playerLayer);
        if (playerCollider != null)
        {
            // Player detected - you can add behavior here like increasing speed or changing direction
            Vector2 directionToPlayer = (playerCollider.transform.position - transform.position).normalized;
            movingRight = directionToPlayer.x > 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle player collision - you can add damage logic here
            Debug.Log("Player hit by snail!");
        }
    }

    private void OnDrawGizmos()
    {
        // Draw patrol range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
        
        // Draw patrol limits
        Gizmos.color = Color.blue;
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(
                new Vector3(startPosition.x - patrolDistance, startPosition.y, 0),
                new Vector3(startPosition.x + patrolDistance, startPosition.y, 0)
            );
        }
    }
} 