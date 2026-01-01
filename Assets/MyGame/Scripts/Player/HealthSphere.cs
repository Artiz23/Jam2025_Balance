using UnityEngine;

public class HealthSphere : MonoBehaviour
{
    [SerializeField] private float healAmount = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.currentHealth >= playerHealth.maxHealth)
                {
                    Debug.Log("Cannot collect health sphere: Player health is full");
                    return;
                }

                playerHealth.Heal(healAmount);
                Debug.Log($"Health sphere collected, restored {healAmount} health");
                Destroy(gameObject);
            }
        }
    }
}