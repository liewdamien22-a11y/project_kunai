using UnityEngine;

public class basicEnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 3.0f;
    [SerializeField] private float damage = 15f;

    void Start()
    {
        // Automatically destroy the projectile after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Fly straight forward along its Z-axis
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if it hit the player using CompareTag
        if (other.CompareTag("Player"))
        {
            // Use TryGetComponent to find player health and deal damage
            if (other.TryGetComponent<PlayerStats>(out var playerHealth))
            {
                playerHealth.TakeDamage(damage);
            }

            // Destroy the projectile upon impact
            Destroy(gameObject);
        }
    }
}