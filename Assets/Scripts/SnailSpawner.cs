using UnityEngine;

public class SnailSpawner : MonoBehaviour
{
    [SerializeField] private GameObject snailPrefab;
    [SerializeField] private float spawnInterval = 7f;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float minSpawnHeight = 5f;
    [SerializeField] private float maxSpawnHeight = 15f;

    private float nextSpawnTime;

    private void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    public void SpawnSnail()
    {
        // Calculate random position within radius
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        float randomHeight = Random.Range(minSpawnHeight, maxSpawnHeight);
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, randomHeight, 0);
        
        // Spawn snail
        Instantiate(snailPrefab, spawnPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        // Visualize spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Visualize height range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * minSpawnHeight, 0.5f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * maxSpawnHeight, 0.5f);
    }
} 