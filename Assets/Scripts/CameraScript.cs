/*using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject Player;
    private Vector3 offset;

    void Start()
    {
        // Make sure player is assigned, otherwise find the player by tag
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }

        // Only set the offset if player was found
        if (Player != null)
        {
            offset = transform.position - Player.transform.position;
        }
        else
        {
            Debug.LogError("Player not found! Please assign a player object.");
        }
    }

    void Update()
    {
        // Ensure player is assigned before updating the camera position
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }

        // If player is found, update the camera position
        if (Player != null)
        {
            transform.position = Player.transform.position + offset;
        }
    }
}*/