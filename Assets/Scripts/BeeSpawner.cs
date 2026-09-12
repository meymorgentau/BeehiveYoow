using UnityEngine;

public class BeeSpawner : MonoBehaviour
{
    [Header("Bee Settings")]
    [SerializeField] private GameObject beePrefab;
    [SerializeField] private int beeCount = 10;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRadius = 0.5f;

    [Header("Hive Settings")]
    [SerializeField] private Transform exitPoint;

    private void Start()
    {
        SpawnBees();
    }

    private void SpawnBees()
    {
        for (int i = 0; i < beeCount; i++)
        {
            Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * spawnRadius;

            Bee bee = Instantiate(
                beePrefab,
                spawnPosition,
                Quaternion.identity,
                transform
            ).GetComponent<Bee>();

            bee.transform.localScale = beePrefab.transform.localScale / transform.localScale.x;
            bee.SetExitPoint(exitPoint);
        }
    }
}