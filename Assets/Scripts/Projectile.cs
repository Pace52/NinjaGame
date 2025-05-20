using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1;

    private void Start()
    {
        // Destroy projectile after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move projectile forward
        transform.Translate(Vector2.right * speed * Time.deltaTime);
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