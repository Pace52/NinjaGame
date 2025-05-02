using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // Initialize the offset based on the initial positions of the player and camera
        offset = transform.position - player.transform.position;
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // Move the camera to the player's position, maintaining the offset
        transform.position = player.transform.position + offset;
    }
}