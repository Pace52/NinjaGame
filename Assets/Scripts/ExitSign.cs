using UnityEngine;

public class ExitSign : MonoBehaviour
{
    [SerializeField] private GameObject snailPrefab;
    [SerializeField] private int snailCount = 20;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private float spawnDelay = 0.2f;

    private bool isPlayerInRange = false;
    private bool hasSpawned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void Update()
    {
        if (isPlayerInRange && !hasSpawned && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(SpawnSnails());
        }
    }

    private System.Collections.IEnumerator SpawnSnails()
    {
        hasSpawned = true;
        
        for (int i = 0; i < snailCount; i++)
        {
            // Calculate random position within radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, spawnHeight, 0);
            
            // Spawn snail
            Instantiate(snailPrefab, spawnPosition, Quaternion.identity);
            
            // Wait before spawning next snail
            yield return new WaitForSeconds(spawnDelay);
        }
    }
} 