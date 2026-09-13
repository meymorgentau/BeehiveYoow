using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bee : MonoBehaviour
{
    [Header("Настройки полёта")]
    [SerializeField] private float flightSpeed = 2f;

    [Header("Свободный полёт")]
    [SerializeField] private float minHeight = 2f;
    [SerializeField] private float maxHeight = 5f;
    [SerializeField] private float targetChangeDistance = 1f;
    [SerializeField] private float searchRadius = 10f;

    [Header("Поиск цветка")]
    [SerializeField] private float flowerSearchRadius = 15f;
    [SerializeField] private float flowerSearchInterval = 1f;
    [SerializeField] private float flowerLandingHeight = 0.8f;

    [Header("Сбор пыльцы")]
    [SerializeField] private float pollenCapacity = 50f;
    [SerializeField] private float pollenPerVisit = 20f;
    [SerializeField] private float pollenCollectionTime = 3f;

    [Header("Улей")]
    [SerializeField] private float hiveDistance = 0.3f;
    [SerializeField] private float unloadTime = 2f;

    [Header("Информация о пчеле")]
    [InspectorName("Текущая пыльца")]
    [SerializeField] private float currentPollen;

    [InspectorName("Заполнение (%)")]
    [SerializeField] private float pollenPercentage;

    [InspectorName("Текущее состояние")]
    [SerializeField] private string currentState;

    private static readonly Dictionary<Transform, Bee> occupiedFlowers =
        new Dictionary<Transform, Bee>();

    private BeeMemory memory;

    private Transform exitPoint;

    private bool hasLeftHive = false;
    private bool returningToHive = false;
    private bool unloadingPollen = false;
    private bool dancePaused = false;

    private Vector3 targetPosition;
    private float flightHeight;

    private Transform targetFlower;
    private float flowerSearchTimer;

    private float pollenCollectionTimer;
    private float carriedPollen;

    private float unloadTimer;

    private bool isCollectingPollen = false;

    public void SetExitPoint(Transform point)
    {
        exitPoint = point;
    }

    public bool HasLeftHive()
    {
        return hasLeftHive;
    }

    public void SetDancePaused(bool paused)
    {
        dancePaused = paused;
    }

    private void Start()
    {
        memory = GetComponent<BeeMemory>();

        flightHeight = Random.Range(minHeight, maxHeight);

        UpdateDebugInformation();
    }

    private void Update()
    {
        UpdateDebugInformation();

        if (dancePaused)
            return;

        if (!hasLeftHive)
        {
            LeaveHive();
            return;
        }

        if (returningToHive)
        {
            ReturnToHive();
            return;
        }

        if (unloadingPollen)
        {
            UnloadPollen();
            return;
        }

        if (isCollectingPollen)
        {
            CollectPollen();
            return;
        }

        if (carriedPollen >= pollenCapacity)
        {
            StartReturningToHive();
            return;
        }

        SearchForFlower();

        FlyAroundTerritory();

        if (targetFlower == null &&
            Vector3.Distance(transform.position, targetPosition) <= targetChangeDistance)
        {
            ChooseNewTarget();
        }

        if (targetFlower != null &&
            Vector3.Distance(transform.position, targetPosition) <= targetChangeDistance)
        {
            StartPollenCollection();
        }
    }

    private void LeaveHive()
    {
        if (exitPoint == null)
            return;

        FlyToExit();

        if (Vector3.Distance(transform.position, exitPoint.position) <= hiveDistance)
        {
            hasLeftHive = true;
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
        if (targetFlower != null)
        {
            Vector3 flowerPosition = targetFlower.position;

            targetPosition = new Vector3(
                flowerPosition.x,
                flowerPosition.y + flowerLandingHeight,
                flowerPosition.z
            );
        }

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

        if (NavMesh.SamplePosition(
            randomPosition,
            out hit,
            searchRadius,
            NavMesh.AllAreas))
        {
            float terrainHeight = hit.position.y;

            flightHeight = Random.Range(minHeight, maxHeight);

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

    private void SearchForFlower()
    {
        flowerSearchTimer -= Time.deltaTime;

        if (flowerSearchTimer > 0f)
            return;

        flowerSearchTimer = flowerSearchInterval;

        if (targetFlower != null)
            return;

        GameObject[] flowers = GameObject.FindGameObjectsWithTag("Flower");

        float closestDistance = flowerSearchRadius;
        Transform closestFlower = null;

        foreach (GameObject flowerObject in flowers)
        {
            Flower flower = flowerObject.GetComponent<Flower>();

            if (flower == null)
                continue;

            if (!flower.HasPollen())
                continue;

            if (occupiedFlowers.ContainsKey(flowerObject.transform))
                continue;

            float distance = Vector3.Distance(
                transform.position,
                flowerObject.transform.position
            );

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestFlower = flowerObject.transform;
        }

        if (closestFlower != null)
        {
            targetFlower = closestFlower;
            occupiedFlowers[targetFlower] = this;
        }
    }

    private void StartPollenCollection()
    {
        if (targetFlower == null)
            return;

        isCollectingPollen = true;
        pollenCollectionTimer = pollenCollectionTime;
    }

    private void CollectPollen()
    {
        if (targetFlower == null)
        {
            isCollectingPollen = false;
            return;
        }

        pollenCollectionTimer -= Time.deltaTime;

        if (pollenCollectionTimer > 0f)
            return;

        Flower flower = targetFlower.GetComponent<Flower>();

        if (flower != null)
        {
            float availablePollen = flower.GetPollen();
            float freeSpace = pollenCapacity - carriedPollen;

            float collectedAmount = Mathf.Min(
                pollenPerVisit,
                availablePollen,
                freeSpace
            );

            flower.TakePollen(collectedAmount);

            carriedPollen += collectedAmount;

            if (memory != null)
            {
                memory.RememberFlower(
                    targetFlower,
                    flower.GetPollen()
                );
            }
        }

        ReleaseFlower();

        isCollectingPollen = false;

        if (carriedPollen >= pollenCapacity)
        {
            StartReturningToHive();
        }
        else
        {
            ChooseNewTarget();
        }
    }

    private void ReleaseFlower()
    {
        if (targetFlower == null)
            return;

        if (occupiedFlowers.TryGetValue(targetFlower, out Bee owner) &&
            owner == this)
        {
            occupiedFlowers.Remove(targetFlower);
        }

        targetFlower = null;
    }

    private void StartReturningToHive()
    {
        ReleaseFlower();

        returningToHive = true;

        if (exitPoint != null)
        {
            targetPosition = exitPoint.position;
        }
    }

    private void ReturnToHive()
    {
        if (exitPoint == null)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            exitPoint.position,
            flightSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, exitPoint.position) <= hiveDistance)
        {
            returningToHive = false;
            unloadingPollen = true;
            unloadTimer = unloadTime;
        }
    }

    private void UnloadPollen()
    {
        unloadTimer -= Time.deltaTime;

        if (unloadTimer > 0f)
            return;

        carriedPollen = 0f;
        unloadingPollen = false;

        ChooseNewTarget();
    }

    private void UpdateDebugInformation()
    {
        currentPollen = carriedPollen;

        if (pollenCapacity > 0f)
        {
            pollenPercentage = carriedPollen / pollenCapacity * 100f;
        }
        else
        {
            pollenPercentage = 0f;
        }

        if (dancePaused)
        {
            currentState = "Общение";
        }
        else if (unloadingPollen)
        {
            currentState = "Выгрузка пыльцы";
        }
        else if (returningToHive)
        {
            currentState = "Возвращение в улей";
        }
        else if (isCollectingPollen)
        {
            currentState = "Сбор пыльцы";
        }
        else if (!hasLeftHive)
        {
            currentState = "Вылет из улья";
        }
        else if (targetFlower != null)
        {
            currentState = "Полет к цветку";
        }
        else
        {
            currentState = "Поиск цветка";
        }
    }

    private void OnDestroy()
    {
        if (targetFlower != null &&
            occupiedFlowers.TryGetValue(targetFlower, out Bee owner) &&
            owner == this)
        {
            occupiedFlowers.Remove(targetFlower);
        }
    }
}