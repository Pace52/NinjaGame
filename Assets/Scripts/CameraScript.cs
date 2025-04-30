using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    Vector3 offset;

    // Update is called once per frame
    void Update()
    {
        if (player = null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        transform.position = player.transform.position - offset;
    }
}
