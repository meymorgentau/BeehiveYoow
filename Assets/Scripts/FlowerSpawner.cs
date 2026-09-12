using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{
    [Header("Flower Settings")]
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField] private int flowerCount = 30;

    [Header("Terrain Settings")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private float minimumHeight = 4.5f;
    [SerializeField] private float waterLevel = 5f;
    [SerializeField] private float groundOffset = 0.02f;

    private void Start()
    {
        GenerateFlowers();
    }

    private void GenerateFlowers()
    {
        for (int i = 0; i < flowerCount; i++)
        {
            float x = Random.Range(0f, terrain.terrainData.size.x);
            float z = Random.Range(0f, terrain.terrainData.size.z);

            Vector3 terrainPosition = terrain.transform.position;

            float worldX = terrainPosition.x + x;
            float worldZ = terrainPosition.z + z;

            Vector3 samplePosition = new Vector3(worldX, 0f, worldZ);
            float groundY = terrain.SampleHeight(samplePosition) + terrainPosition.y;

            if (groundY < minimumHeight)
                continue;

            if (groundY <= waterLevel)
                continue;

            GameObject flower = Instantiate(
                flowerPrefab,
                new Vector3(worldX, groundY, worldZ),
                Quaternion.identity,
                transform
            );

            PlaceOnGround(flower, groundY);
        }
    }

    private void PlaceOnGround(GameObject flower, float groundY)
    {
        Renderer flowerRenderer = flower.GetComponentInChildren<Renderer>();

        if (flowerRenderer == null)
            return;

        float bottomOffset = flowerRenderer.bounds.min.y - flower.transform.position.y;

        flower.transform.position += Vector3.up * (-bottomOffset + groundOffset);
    }
}