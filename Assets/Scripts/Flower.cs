using UnityEngine;

public class Flower : MonoBehaviour
{
    [Header("Pollen Settings")]
    [SerializeField] private float maximumPollen = 100f;
    [SerializeField] private float currentPollen = 100f;
    [SerializeField] private float regenerationSpeed = 5f;

    private void Update()
    {
        RegeneratePollen();
    }

    private void RegeneratePollen()
    {
        if (currentPollen >= maximumPollen)
            return;

        currentPollen += regenerationSpeed * Time.deltaTime;
        currentPollen = Mathf.Min(currentPollen, maximumPollen);
    }

    public bool HasPollen()
    {
        return currentPollen > 0f;
    }

    public void TakePollen(float amount)
    {
        currentPollen = Mathf.Max(0f, currentPollen - amount);
    }

    public float GetPollen()
    {
        return currentPollen;
    }
}