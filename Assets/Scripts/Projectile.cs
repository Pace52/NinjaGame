using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1;

    private Vector2 direction = Vector2.right;

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
        // Flip sprite if moving left
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void Start()
    {
        // Destroy projectile after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move projectile in its direction
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit a snail
        if (other.CompareTag("Enemy"))
        {
            SnailEnemy snail = other.GetComponent<SnailEnemy>();
            if (snail != null)
            {
                snail.TakeDamage();
            }
        }

        // Destroy projectile on hit
        Destroy(gameObject);
    }
} 