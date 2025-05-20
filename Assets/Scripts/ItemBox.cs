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
    [SerializeField] private float gravityScale = 1f;

    private bool isFalling = false;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector3 initialPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        initialPosition = transform.position;
        
        // Initialize ground layer
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex == -1)
        {
            Debug.LogError("Ground layer not found! Please create a layer named 'Ground' in the Layer settings.");
        }
        else
        {
            groundLayer = 1 << groundLayerIndex;
        }
        
        // Initially disable gravity and make kinematic
        rb.gravityScale = 0;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Make sure the collider is not a trigger initially
        boxCollider.isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling && collision.gameObject.CompareTag("Player"))
        {
            // Check if player hit from below
            ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
            collision.GetContacts(contacts);
            
            foreach (ContactPoint2D contact in contacts)
            {
                // If the contact point is below the center of the box, player hit from below
                if (contact.point.y < transform.position.y)
                {
                    StartFalling();
                    break;
                }
            }
        }
        // Check if we hit the ground
        else if (isFalling && ((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            SpawnCoins();
            Destroy(gameObject);
        }
    }

    private void StartFalling()
    {
        isFalling = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = gravityScale;
        rb.linearVelocity = new Vector2(0, -fallSpeed);
        boxCollider.isTrigger = false;
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