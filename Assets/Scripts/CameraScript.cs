using UnityEngine;
public gameObject player;
public class NewMonoBehaviourScript : MonoBehaviour
{
    Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = player.transfrom.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transfrom.position = player.transform.position + offset;
    }
}
