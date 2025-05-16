using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [Header("Box Settings")]
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinsToSpawn = 3;
    [SerializeField] private float coinSpawnForce = 5f;
    [SerializeField] private float coinSpreadAngle = 45f;

    private bool isFalling = false;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Initially disable gravity
        rb.gravityScale = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFalling && other.CompareTag("Player"))
        {
            StartFalling();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we hit the ground
        if (isFalling && ((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            SpawnCoins();
            // Destroy the box
            Destroy(gameObject);
        }
    }

    private void StartFalling()
    {
        isFalling = true;
        rb.gravityScale = 1;
    }

    private void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogError("Coin prefab not assigned to ItemBox!");
            return;
        }

        // Calculate the spread between coins
        float angleStep = (coinSpreadAngle * 2) / (coinsToSpawn - 1);
        float startAngle = -coinSpreadAngle;

        for (int i = 0; i < coinsToSpawn; i++)
        {
            // Create coin
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();

            if (coinRb != null)
            {
                // Calculate direction for this coin
                float angle = startAngle + (angleStep * i);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;

                // Apply force
                coinRb.AddForce(direction * coinSpawnForce, ForceMode2D.Impulse);
            }

            // Make sure coin is tagged properly
            coin.tag = "Coin";
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the coin spread in the editor
        if (Application.isPlaying && isFalling)
        {
            Gizmos.color = Color.yellow;
            float angleStep = (coinSpreadAngle * 2) / (coinsToSpawn - 1);
            float startAngle = -coinSpreadAngle;

            for (int i = 0; i < coinsToSpawn; i++)
            {
                float angle = startAngle + (angleStep * i);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
                Gizmos.DrawRay(transform.position, direction);
            }
        }
    }
} 