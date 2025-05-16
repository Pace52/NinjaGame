using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject Player;
    private Vector3 offset;
    private Camera cam;
    public float smoothSpeed = 0.125f; // Smoothing factor for camera movement
    public Vector3 positionOffset = new Vector3(0, 1, -10); // Offset from player position

    void Start()
    {
        // Get camera component
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            // Set to orthographic mode
            cam.orthographic = true;
            cam.orthographicSize = 5f; // Adjust this value to change zoom level
        }

        // Make sure player is assigned, otherwise find the player by tag
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            if (Player == null)
            {
                Debug.LogError("Player not found! Make sure the Player object has the 'Player' tag.");
                return;
            }
        }

        // Set initial position
        transform.position = Player.transform.position + positionOffset;
    }

    void LateUpdate()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            if (Player == null) return;
        }

        // Calculate desired position
        Vector3 desiredPosition = Player.transform.position + positionOffset;
        
        // Smoothly move camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}