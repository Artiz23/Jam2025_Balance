using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] public float maxHealth = 12f;
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite almostEmptyHeart;
    [SerializeField] private Sprite emptyHeart;


    [SerializeField] private float beatAmplitude = 0.1f;
    [SerializeField] private float fullHeartBeatSpeed = 1f;
    [SerializeField] private float halfHeartBeatSpeed = 1.5f;
    [SerializeField] private float almostEmptyHeartBeatSpeed = 2f;

    public float currentHealth;
    public float MaxHealth => maxHealth;

    private Vector3[] baseScales;
    private float[] beatTimers;

    private void Start()
    {
        currentHealth = maxHealth;
        baseScales = new Vector3[heartImages.Length];
        beatTimers = new float[heartImages.Length];
        for (int i = 0; i < heartImages.Length; i++)
        {
            baseScales[i] = heartImages[i].transform.localScale;
            beatTimers[i] = 0f;
        }
        UpdateHealthUI();
    }

    private void Update()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            float beatSpeed = GetBeatSpeed(heartImages[i].sprite);
            if (beatSpeed > 0f)
            {
                beatTimers[i] += Time.deltaTime * beatSpeed;
                float scaleFactor = 1f + Mathf.Sin(beatTimers[i] * Mathf.PI * 2f) * beatAmplitude;
                heartImages[i].transform.localScale = baseScales[i] * scaleFactor;
            }
            else
            {
                heartImages[i].transform.localScale = baseScales[i];
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"Damage taken: {damage}, Current health: {currentHealth}");
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"Health restored: {amount}, Current health: {currentHealth}");
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (heartImages == null || heartImages.Length != 4)
        {
            Debug.LogWarning("Heart images not assigned or incorrect length");
            return;
        }

        int totalHearts = heartImages.Length;
        float healthPerHeart = maxHealth / totalHearts;

        for (int i = 0; i < totalHearts; i++)
        {
            float heartHealth = Mathf.Clamp(currentHealth - (i * healthPerHeart), 0f, healthPerHeart);

            if (heartHealth >= 3f)
            {
                heartImages[i].sprite = fullHeart;
            }
            else if (heartHealth >= 2f)
            {
                heartImages[i].sprite = halfHeart;
            }
            else if (heartHealth >= 1f)
            {
                heartImages[i].sprite = almostEmptyHeart;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }
        }
    }

    private float GetBeatSpeed(Sprite currentSprite)
    {
        if (currentSprite == fullHeart)
            return 0f;
        else if (currentSprite == halfHeart)
            return halfHeartBeatSpeed;
        else if (currentSprite == almostEmptyHeart)
            return almostEmptyHeartBeatSpeed;
        else
            return 0f;
    }

    private void Die()
    {
        Debug.Log("Player died!");
    }
}