using UnityEngine;
using UnityEngine.AI;

public class CapturePoint : MonoBehaviour
{
    [SerializeField] private float moveInterval = 60f;
    [SerializeField] private float moveRadius = 50f;

    private float moveTimer;

    private void Start()
    {
        moveTimer = moveInterval;
    }

    private void Update()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0)
        {
            MoveToNewPosition();
            moveTimer = moveInterval;
        }
    }

    private void MoveToNewPosition()
    {
        Vector3 newPosition;
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
            newPosition = new Vector3(transform.position.x + randomCircle.x, transform.position.y, transform.position.z + randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(newPosition, out hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                return;
            }
        }
        Debug.LogWarning("Could not find valid NavMesh position for Capture Point!");
    }
}