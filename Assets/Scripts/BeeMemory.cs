using System.Collections.Generic;
using UnityEngine;

public class BeeMemory : MonoBehaviour
{
    [System.Serializable]
    public class FlowerMemory
    {
        public Transform flower;
        public float pollenAmount;
    }

    [Header("Память пчелы")]
    [SerializeField] private List<FlowerMemory> knownFlowers =
        new List<FlowerMemory>();

    public void RememberFlower(Transform flower, float pollenAmount)
    {
        if (flower == null)
            return;

        foreach (FlowerMemory memory in knownFlowers)
        {
            if (memory.flower == flower)
            {
                memory.pollenAmount = pollenAmount;
                return;
            }
        }

        FlowerMemory newMemory = new FlowerMemory
        {
            flower = flower,
            pollenAmount = pollenAmount
        };

        knownFlowers.Add(newMemory);
    }

    public bool KnowsFlower(Transform flower)
    {
        if (flower == null)
            return false;

        foreach (FlowerMemory memory in knownFlowers)
        {
            if (memory.flower == flower)
                return true;
        }

        return false;
    }

    public float GetRememberedPollen(Transform flower)
    {
        if (flower == null)
            return 0f;

        foreach (FlowerMemory memory in knownFlowers)
        {
            if (memory.flower == flower)
                return memory.pollenAmount;
        }

        return 0f;
    }
}