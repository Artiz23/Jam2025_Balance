using UnityEngine;
using UnityEngine.AI;

public class MobAI : MonoBehaviour
{
    public enum MobType { LightMob, DarkMob }
    public enum MobState { Patrol, Aggro }

    [SerializeField] private MobType mobType;
    [SerializeField] private float updateRate = 0.1f;
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float stoppingDistance = 1.5f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float agroRange = 10f;
    [SerializeField] private float agroTimeout = 5f;
    [SerializeField] private string pointControlTag = "ControlPoint";

    private NavMeshAgent agent;
    private Transform player;
    private Transform capturePoint;
    private PlayerHealth playerHealth;
    private Transform currentTarget; // Текущая цель (игрок или другой моб)
    private MobAI targetMob; // MobAI цели, если это моб
    private float lastAttackTime;
    private float lastUpdateTime;
    private float lastDamageTime;
    private float lastInRangeTime;
    private bool canAttackTarget;
    private MobState currentState = MobState.Patrol;
    public float health = 10f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Player not found!");
            return;
        }

        GameObject captureObj = GameObject.FindGameObjectWithTag(pointControlTag);
        if (captureObj != null)
        {
            capturePoint = captureObj.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Capture Point not found!");
            return;
        }

        gameObject.tag = mobType == MobType.LightMob ? "LightMob" : "DarkMob";

        lastInRangeTime = -agroTimeout;
        lastDamageTime = -agroTimeout;

        float initialDistanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (initialDistanceToPlayer <= agroRange)
        {
            currentState = MobState.Aggro;
            currentTarget = player;
            targetMob = null;
            lastInRangeTime = Time.time;
        }
        else
        {
            MobAI enemyMob = FindNearestEnemyMob();
            if (enemyMob != null && Vector3.Distance(transform.position, enemyMob.transform.position) <= agroRange)
            {
                currentState = MobState.Aggro;
                currentTarget = enemyMob.transform;
                targetMob = enemyMob;
                lastInRangeTime = Time.time;
            }
        }

        UpdatePath();
    }

    private void Update()
    {
        if (player == null || playerHealth == null || capturePoint == null) return;

        if (currentState == MobState.Aggro && (currentTarget == null || !currentTarget.gameObject.activeInHierarchy))
        {
            currentState = MobState.Patrol;
            currentTarget = null;
            targetMob = null;
            canAttackTarget = false;
            UpdatePath();
        }
        else if (currentState == MobState.Aggro && currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget > agroRange && Time.time - lastDamageTime > agroTimeout)
            {
                float timeSinceOutOfRange = Time.time - lastInRangeTime;
                if (timeSinceOutOfRange > agroTimeout)
                {
                    currentState = MobState.Patrol;
                    currentTarget = null;
                    targetMob = null;
                    canAttackTarget = false;
                    UpdatePath();
                }
            }
            else if (distanceToTarget <= agroRange)
            {
                lastInRangeTime = Time.time;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (currentState != MobState.Aggro && distanceToPlayer <= agroRange)
        {
            currentState = MobState.Aggro;
            currentTarget = player;
            targetMob = null;
            lastInRangeTime = Time.time;
        }
        else if (currentState != MobState.Aggro)
        {
            MobAI enemyMob = FindNearestEnemyMob();
            if (enemyMob != null && Vector3.Distance(transform.position, enemyMob.transform.position) <= agroRange)
            {
                currentState = MobState.Aggro;
                currentTarget = enemyMob.transform;
                targetMob = enemyMob;
                lastInRangeTime = Time.time;
            }
        }

        if (Time.time - lastUpdateTime >= updateRate)
        {
            UpdatePath();
            lastUpdateTime = Time.time;
        }

        if (canAttackTarget && currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            if (Time.time - lastAttackTime >= attackRate)
            {
                if (currentTarget == player && playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
                else if (targetMob != null && targetMob.gameObject.activeInHierarchy)
                {
                    targetMob.TakeDamage(attackDamage, this);
                    if (targetMob.health <= 0f)
                    {
                        currentState = MobState.Patrol;
                        currentTarget = null;
                        targetMob = null;
                        canAttackTarget = false;
                        UpdatePath();
                    }
                }
                lastAttackTime = Time.time;
            }
        }
    }

    private MobAI FindNearestEnemyMob()
    {
        MobAI[] allMobs = FindObjectsOfType<MobAI>();
        MobAI nearestEnemy = null;
        float closestDistance = Mathf.Infinity;
        string enemyTag = mobType == MobType.LightMob ? "DarkMob" : "LightMob";

        foreach (MobAI mob in allMobs)
        {
            if (mob == this || mob.tag != enemyTag || !mob.gameObject.activeInHierarchy) continue;
            float distance = Vector3.Distance(transform.position, mob.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestEnemy = mob;
            }
        }

        return nearestEnemy;
    }

    public void TakeDamage(float damage, MobAI attacker = null)
    {
        health -= damage;
        lastDamageTime = Time.time;
        currentState = MobState.Aggro;

        if (attacker != null && attacker.mobType != mobType && attacker.gameObject.activeInHierarchy)
        {
            currentTarget = attacker.transform;
            targetMob = attacker;
        }
        else
        {
            currentTarget = player;
            targetMob = null;
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void UpdatePath()
    {
        if (currentState == MobState.Patrol)
        {
            agent.SetDestination(capturePoint.position);
        }
        else if (currentState == MobState.Aggro && currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            agent.SetDestination(currentTarget.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == currentTarget || other.CompareTag("Player") || other.CompareTag(mobType == MobType.LightMob ? "DarkMob" : "LightMob"))
        {
            canAttackTarget = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform == currentTarget || other.CompareTag("Player") || other.CompareTag(mobType == MobType.LightMob ? "DarkMob" : "LightMob"))
        {
            canAttackTarget = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == currentTarget || other.CompareTag("Player") || other.CompareTag(mobType == MobType.LightMob ? "DarkMob" : "LightMob"))
        {
            canAttackTarget = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, agroRange);

        Collider triggerCollider = GetComponentInChildren<Collider>();
        if (triggerCollider != null && triggerCollider.isTrigger)
        {
            Gizmos.color = Color.yellow;
            if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(triggerCollider.transform.position, sphere.radius);
            }
            else if (triggerCollider is BoxCollider box)
            {
                Gizmos.DrawWireCube(triggerCollider.transform.position, box.size);
            }
        }

        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            Vector3[] pathCorners = agent.path.corners;
            for (int i = 1; i < pathCorners.Length; i++)
            {
                Gizmos.DrawLine(pathCorners[i - 1], pathCorners[i]);
            }
        }

        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"State: {currentState}");
        if (currentState == MobState.Aggro && currentTarget != null && currentTarget.gameObject.activeInHierarchy)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"Target: {(currentTarget == player ? "Player" : currentTarget.name)}");
        if (currentState == MobState.Aggro && currentTarget != null && currentTarget.gameObject.activeInHierarchy && Vector3.Distance(transform.position, currentTarget.position) > agroRange)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, 
                $"Chase Time: {(agroTimeout - (Time.time - lastInRangeTime)):F1}s");
        #endif
    }
}