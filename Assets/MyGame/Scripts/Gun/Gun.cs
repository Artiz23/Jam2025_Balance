using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform output;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] shootClips;
    [SerializeField] private float fireRate = 0.2f;

    private float lastShotTime;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShotTime + fireRate)
        {
            Shoot();
            PlayRandomShootSound();
            lastShotTime = Time.time;
        }
    }

    private void Shoot()
    {
        Instantiate(bulletPrefab, output.position, output.rotation);
    }

    private void PlayRandomShootSound()
    {
        if (shootClips.Length > 0)
        {
            int randomIndex = Random.Range(0, shootClips.Length);
            audioSource.PlayOneShot(shootClips[randomIndex]);
        }
    }
}