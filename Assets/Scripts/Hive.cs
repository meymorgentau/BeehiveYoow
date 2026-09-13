using UnityEngine;

public class Hive : MonoBehaviour
{
    [Header("Pollen")]
    [SerializeField] private float totalPollen;

    public float TotalPollen => totalPollen;

    public void AddPollen(float amount)
    {
        if (amount <= 0f)
            return;

        totalPollen += amount;
    }
}