using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifetime = 10f;  // Max lifetime (seconds) before auto-destroy

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Auto-destroy if lifetime exceeded
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }

    // Detect collision with any object
    void OnCollisionEnter(Collision collision)
    {
        // Check if hit something other than the player
        if (collision.gameObject.tag != "Player")
        {
            // Destroy bullet on impact
            Destroy(gameObject);
        }
    }

    // Optional: trigger collision for physics-based objects
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            Destroy(gameObject);
        }
    }
}
