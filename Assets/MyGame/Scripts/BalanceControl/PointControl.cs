using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class PointControl : MonoBehaviour
{
    public BalanceBarController balanceBar;
    public float captureSpeed = 0.5f;

    private List<GameObject> playersInTrigger = new List<GameObject>();
    private List<GameObject> lightMobsInTrigger = new List<GameObject>();
    private List<GameObject> darkMobsInTrigger = new List<GameObject>();
    
    [SerializeField] private float moveInterval = 60f;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float checkRadius = 5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private GameObject lightMobPrefab;
    [SerializeField] private GameObject darkMobPrefab;
    [SerializeField] private Transform[] lightSpawnPoints;
    [SerializeField] private Transform[] darkSpawnPoints;
    [SerializeField] private int initialMobCount = 3;
    private int currentWave = 1;
    private float moveTimer;
    private List<GameObject> activeMobs = new List<GameObject>();

    private void Start()
    {
        moveTimer = moveInterval;
        SpawnInitialMobs();
        MoveToNewPosition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        int segments = 50;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            Vector3 center = spawnPoint.position;
            Vector3 prevPoint = center + new Vector3(checkRadius, 0, 0);
            float angleStep = 2 * Mathf.PI / segments;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep;
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle) * checkRadius,
                    0,
                    Mathf.Sin(angle) * checkRadius
                );
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }

    private void SpawnInitialMobs()
    {
        SpawnWave(initialMobCount);
    }

    private void SpawnWave(int mobCount)
    {
        ClearMobs();

        int totalMobs = mobCount + (currentWave - 1) * 2;
        int lightMobCount = Random.Range(1, totalMobs);
        int darkMobCount = totalMobs - lightMobCount;

        for (int i = 0; i < lightMobCount; i++)
        {
            if (lightSpawnPoints.Length > 0)
            {
                Transform spawnPoint = lightSpawnPoints[Random.Range(0, lightSpawnPoints.Length)];
                GameObject mob = Instantiate(lightMobPrefab, spawnPoint.position, Quaternion.identity);
                activeMobs.Add(mob);
            }
        }

        for (int i = 0; i < darkMobCount; i++)
        {
            if (darkSpawnPoints.Length > 0)
            {
                Transform spawnPoint = darkSpawnPoints[Random.Range(0, darkSpawnPoints.Length)];
                GameObject mob = Instantiate(darkMobPrefab, spawnPoint.position, Quaternion.identity);
                activeMobs.Add(mob);
            }
        }
    }

    private void ClearMobs()
    {
        foreach (GameObject mob in activeMobs)
        {
            if (mob != null)
                Destroy(mob);
        }
        activeMobs.Clear();
        lightMobsInTrigger.Clear();
        darkMobsInTrigger.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playersInTrigger.Contains(other.gameObject))
        {
            playersInTrigger.Add(other.gameObject);
        }
        else if (other.CompareTag("LightMob") && !lightMobsInTrigger.Contains(other.gameObject))
        {
            lightMobsInTrigger.Add(other.gameObject);
        }
        else if (other.CompareTag("DarkMob") && !darkMobsInTrigger.Contains(other.gameObject))
        {
            darkMobsInTrigger.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTrigger.Remove(other.gameObject);
        }
        else if (other.CompareTag("LightMob"))
        {
            lightMobsInTrigger.Remove(other.gameObject);
        }
        else if (other.CompareTag("DarkMob"))
        {
            darkMobsInTrigger.Remove(other.gameObject);
        }
    }

    private void Update()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0)
        {
            MoveToNewPosition();
            currentWave++;
            SpawnWave(initialMobCount);
            moveTimer = moveInterval;
        }

        playersInTrigger.RemoveAll(obj => obj == null);
        lightMobsInTrigger.RemoveAll(obj => obj == null);
        darkMobsInTrigger.RemoveAll(obj => obj == null);

        float playerWeight = playersInTrigger.Count * 0.5f;
        float lightMobWeight = lightMobsInTrigger.Count * 1f;
        float darkMobWeight = darkMobsInTrigger.Count * 1f;

        float lightSideForce = lightMobWeight;
        float darkSideForce = darkMobWeight;
        float neutralForce = playerWeight;

        float targetBalance;
        float forceDifference;

        if (neutralForce > 0 && lightSideForce == 0 && darkSideForce == 0)
        {
            targetBalance = 0f;
            forceDifference = neutralForce;
        }
        else if (lightSideForce > 0 || darkSideForce > 0 || neutralForce > 0)
        {
            forceDifference = lightSideForce - darkSideForce;
            if (neutralForce > 0)
            {
                if (forceDifference != 0)
                {
                    forceDifference = forceDifference > 0 ? Mathf.Max(0, forceDifference - neutralForce) : Mathf.Min(0, forceDifference + neutralForce);
                }
                else
                {
                    forceDifference = 0f;
                }
            }

            if (forceDifference > 0)
            {
                targetBalance = 1f;
            }
            else if (forceDifference < 0)
            {
                targetBalance = -1f;
            }
            else
            {
                targetBalance = 0f;
                forceDifference = 0f;
            }
        }
        else
        {
            targetBalance = 0f;
            forceDifference = 0f;
        }


        if (Mathf.Abs(forceDifference) > 0.01f)
        {
            float currentBalance = balanceBar.GetBalanceValue();
            float step = captureSpeed * Mathf.Abs(forceDifference) * Time.deltaTime;

            if (currentBalance < targetBalance)
            {
                balanceBar.UpdateBar(Mathf.Min(currentBalance + step, targetBalance));
            }
            else if (currentBalance > targetBalance)
            {
                balanceBar.UpdateBar(Mathf.Max(currentBalance - step, targetBalance));
            }
        }
    }

    private void MoveToNewPosition()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned for PointControl!");
            return;
        }

        List<Transform> validSpawnPoints = new List<Transform>();
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, checkRadius, obstacleLayer);
            if (colliders.Length == 0)
            {
                validSpawnPoints.Add(spawnPoint);
            }
        }

        if (validSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No valid spawn points available for PointControl!");
            return;
        }

        Transform selectedSpawnPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        transform.position = selectedSpawnPoint.position;
    }

    private string GetCaptureType(float target)
    {
        if (target == 0f) return "Игрок";
        if (target == 1f) return "LightMob";
        return "DarkMob";
    }
}