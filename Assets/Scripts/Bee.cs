using UnityEngine;
using UnityEngine.AI;

public class Bee : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float flightSpeed = 2f;

    [Header("Free Flight")]
    [SerializeField] private float minHeight = 2f;
    [SerializeField] private float maxHeight = 5f;
    [SerializeField] private float targetChangeDistance = 1f;
    [SerializeField] private float searchRadius = 10f;

    private Transform exitPoint;
    private bool hasLeftHive = false;

    private Vector3 targetPosition;
    private float flightHeight;

    public void SetExitPoint(Transform point)
    {
        exitPoint = point;
    }

    private void Start()
    {
        flightHeight = Random.Range(minHeight, maxHeight);
    }

    private void Update()
    {
        if (!hasLeftHive)
        {
            if (exitPoint == null)
                return;

            FlyToExit();

            if (Vector3.Distance(transform.position, exitPoint.position) < 0.1f)
            {
                hasLeftHive = true;
                ChooseNewTarget();
            }

            return;
        }

        FlyAroundTerritory();

        if (Vector3.Distance(transform.position, targetPosition) <= targetChangeDistance)
        {
            ChooseNewTarget();
        }
    }

    private void FlyToExit()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            exitPoint.position,
            flightSpeed * Time.deltaTime
        );
    }

    private void FlyAroundTerritory()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            flightSpeed * Time.deltaTime
        );
    }

    private void ChooseNewTarget()
    {
        Vector3 randomPosition = transform.position + new Vector3(
            Random.Range(-searchRadius, searchRadius),
            0f,
            Random.Range(-searchRadius, searchRadius)
        );

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPosition, out hit, searchRadius, NavMesh.AllAreas))
        {
            float terrainHeight = hit.position.y;

            targetPosition = new Vector3(
                hit.position.x,
                terrainHeight + flightHeight,
                hit.position.z
            );
        }
        else
        {
            targetPosition = new Vector3(
                randomPosition.x,
                transform.position.y,
                randomPosition.z
            );
        }
    }
}